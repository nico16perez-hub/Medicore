using Medicore.Api.Data;
using Medicore.Api.Dtos;
using Medicore.Api.Models;
using Medicore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Medicore.Api.Controllers;

[ApiController]
[Route("api/historia-clinica")]
[Authorize(Roles = "paciente,doctor")]
public class HistoriaClinicaController : ControllerBase
{
    private readonly MedicoreDbContext context;

    public HistoriaClinicaController(MedicoreDbContext context)
    {
        this.context = context;
    }

    [HttpGet]
    public async Task<ActionResult<object>> ObtenerRegistros(
        [FromQuery] string? tipo = null,
        [FromQuery] string? busqueda = null,
        [FromQuery] int? pacienteUsuarioId = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 5,
        CancellationToken cancellationToken = default)
    {
        if (pagina < 1)
        {
            pagina = 1;
        }

        if (tamano < 1)
        {
            tamano = 5;
        }

        if (tamano > 20)
        {
            tamano = 20;
        }

        var rol = ObtenerRol();
        if (rol == "doctor" &&
            pacienteUsuarioId.HasValue &&
            !await DoctorAtiendePacienteAsync(pacienteUsuarioId.Value, cancellationToken))
        {
            return ProblemaNoEncontrado("El paciente no tiene una atención asignada con este doctor.");
        }

        var query = context.HistoriaClinica
            .AsNoTracking()
            .AsQueryable();
        query = FiltrarPorUsuario(query, pacienteUsuarioId);

        if (!string.IsNullOrWhiteSpace(tipo) && !string.Equals(tipo, "Todos", StringComparison.OrdinalIgnoreCase))
        {
            var tipoFiltrado = tipo.Trim();
            query = query.Where(registro => registro.Tipo == tipoFiltrado);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var valor = busqueda.Trim();
            query = query.Where(registro =>
                registro.Tipo.Contains(valor) ||
                registro.Valor.Contains(valor) ||
                registro.Notas.Contains(valor));
        }

        var total = await query.CountAsync(cancellationToken);
        var registros = await query
            .OrderByDescending(registro => registro.Fecha)
            .ThenByDescending(registro => registro.Id)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(registro => new RegistroHistoriaClinicaResumenDto(
                registro.Id, registro.PacienteUsuarioId, registro.DoctorUsuarioId,
                registro.Fecha, registro.Tipo,
                registro.Valor, registro.Notas))
            .ToListAsync(cancellationToken);

        var totalPaginas = total == 0 ? 0 : (int)Math.Ceiling(total / (double)tamano);

        return Ok(new
        {
            registros,
            pagina,
            tamano,
            total,
            totalPaginas,
            filtros = new
            {
                busqueda = busqueda ?? string.Empty,
                tipo = tipo ?? "Todos"
            }
        });
    }

    [Authorize(Roles = "doctor")]
    [HttpGet("pacientes")]
    public async Task<ActionResult<IReadOnlyList<PacienteHistoriaDto>>> ObtenerPacientesAsignados(
        CancellationToken cancellationToken)
    {
        var doctorId = AuthClaims.ObtenerUsuarioId(User);
        var turnos = await context.Turnos
            .AsNoTracking()
            .Where(turno => turno.DoctorUsuarioId == doctorId &&
                turno.PacienteUsuarioId.HasValue &&
                (turno.Estado == TurnoEstados.Confirmado ||
                    turno.Estado == TurnoEstados.Reprogramado))
            .Select(turno => new
            {
                PacienteUsuarioId = turno.PacienteUsuarioId!.Value,
                turno.Fecha
            })
            .ToListAsync(cancellationToken);

        var fechaHoy = DateOnly.FromDateTime(DateTime.Today);
        var proximosTurnos = turnos
            .Where(turno => turno.Fecha >= fechaHoy)
            .GroupBy(turno => turno.PacienteUsuarioId)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => (DateOnly?)grupo.Min(turno => turno.Fecha));
        var pacienteIds = turnos
            .Select(turno => turno.PacienteUsuarioId)
            .Distinct()
            .ToArray();
        var pacientes = await context.Usuarios
            .AsNoTracking()
            .Where(usuario => pacienteIds.Contains(usuario.Id) &&
                usuario.Activo &&
                usuario.Rol != null &&
                usuario.Rol.Codigo == "paciente")
            .OrderBy(usuario => usuario.Email)
            .Select(usuario => new { usuario.Id, usuario.Email })
            .ToListAsync(cancellationToken);

        return Ok(pacientes.Select(paciente => new PacienteHistoriaDto(
            paciente.Id,
            paciente.Email,
            proximosTurnos.GetValueOrDefault(paciente.Id))));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RegistroHistoriaClinicaResumenDto>> ObtenerRegistro(int id)
    {
        var consulta = context.HistoriaClinica
            .AsNoTracking()
            .Where(registro => registro.Id == id);
        consulta = FiltrarPorUsuario(consulta);
        var registro = await consulta
            .Select(registro => new RegistroHistoriaClinicaResumenDto(
                registro.Id, registro.PacienteUsuarioId, registro.DoctorUsuarioId,
                registro.Fecha, registro.Tipo,
                registro.Valor, registro.Notas))
            .SingleOrDefaultAsync();

        return registro is null
            ? ProblemaNoEncontrado("El registro de historia clínica solicitado no existe.")
            : Ok(registro);
    }

    [Authorize(Roles = "doctor")]
    [HttpPost]
    public async Task<ActionResult<RegistroHistoriaClinicaResumenDto>> CrearRegistro(
        RegistroHistoriaClinicaRequest request)
    {
        var rol = ObtenerRol();
        if (!await AsignacionValidaAsync(request, rol))
        {
            return ProblemaValidacion("El paciente o doctor asignado no existe, está inactivo o no tiene el rol correspondiente.");
        }

        var validacion = await ValidarRegistroAsync(request);
        if (validacion is not null)
        {
            return validacion;
        }

        var registro = new RegistroHistoriaClinica();
        Aplicar(request, registro);
        if (rol == "doctor")
        {
            registro.DoctorUsuarioId = AuthClaims.ObtenerUsuarioId(User);
        }

        context.HistoriaClinica.Add(registro);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerRegistro), new { id = registro.Id }, Mapear(registro));
    }

    [Authorize(Roles = "doctor")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<RegistroHistoriaClinicaResumenDto>> ModificarRegistro(
        int id,
        RegistroHistoriaClinicaRequest request)
    {
        var registro = await context.HistoriaClinica.FindAsync(id);
        if (registro is null)
        {
            return ProblemaNoEncontrado("El registro de historia clínica solicitado no existe.");
        }

        if (ObtenerRol() == "doctor" &&
            registro.DoctorUsuarioId != AuthClaims.ObtenerUsuarioId(User))
        {
            return ProblemaNoEncontrado("El registro de historia clínica solicitado no existe.");
        }

        if (ObtenerRol() == "doctor")
        {
            request.PacienteUsuarioId = null;
            request.DoctorUsuarioId = null;
        }

        var validacion = await ValidarRegistroAsync(request, id);
        if (validacion is not null)
        {
            return validacion;
        }

        Aplicar(request, registro, mantenerAsignacionesSinCambio: true);
        await context.SaveChangesAsync();

        return Ok(Mapear(registro));
    }

    [Authorize(Roles = "doctor")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarRegistro(int id)
    {
        var registro = await context.HistoriaClinica.FindAsync(id);
        if (registro is null)
        {
            return ProblemaNoEncontrado("El registro de historia clínica solicitado no existe.");
        }

        if (ObtenerRol() == "doctor" &&
            registro.DoctorUsuarioId != AuthClaims.ObtenerUsuarioId(User))
        {
            return ProblemaNoEncontrado("El registro de historia clínica solicitado no existe.");
        }

        context.HistoriaClinica.Remove(registro);
        await context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<ActionResult<RegistroHistoriaClinicaResumenDto>?> ValidarRegistroAsync(RegistroHistoriaClinicaRequest request, int? id = null)
    {
        if (!ModelState.IsValid)
        {
            var detalle = string.Join(" ", ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "El valor enviado no es válido."
                    : error.ErrorMessage));

            return ProblemaValidacion(detalle);
        }

        if (request.Fecha == default)
        {
            return ProblemaValidacion("La fecha del registro es obligatoria.");
        }

        var tipo = request.Tipo.Trim();
        var valor = request.Valor.Trim();
        var notas = request.Notas.Trim();

        if (string.IsNullOrWhiteSpace(tipo))
        {
            return ProblemaValidacion("El tipo de registro es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(valor))
        {
            return ProblemaValidacion("El valor del registro es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(notas))
        {
            return ProblemaValidacion("Las notas del registro son obligatorias.");
        }

        var pacienteUsuarioId = request.PacienteUsuarioId;
        if (!pacienteUsuarioId.HasValue && id.HasValue)
        {
            pacienteUsuarioId = await context.HistoriaClinica
                .Where(registro => registro.Id == id.Value)
                .Select(registro => registro.PacienteUsuarioId)
                .SingleOrDefaultAsync();
        }

        var yaExiste = await context.HistoriaClinica.AnyAsync(registro =>
            registro.Id != (id ?? 0) &&
            registro.PacienteUsuarioId == pacienteUsuarioId &&
            registro.Fecha == request.Fecha &&
            registro.Tipo == tipo &&
            registro.Valor == valor);

        if (yaExiste)
        {
            return ProblemaValidacion("Ya existe un registro igual para ese tipo y valor en la misma fecha.");
        }

        return null;
    }

    private IQueryable<RegistroHistoriaClinica> FiltrarPorUsuario(
        IQueryable<RegistroHistoriaClinica> query,
        int? pacienteUsuarioId = null)
    {
        var usuarioId = AuthClaims.ObtenerUsuarioId(User);
        var consulta = ObtenerRol() switch
        {
            "paciente" => query.Where(registro => registro.PacienteUsuarioId == usuarioId),
            "doctor" => query.Where(registro =>
                registro.PacienteUsuarioId.HasValue &&
                context.Turnos.Any(turno =>
                    turno.DoctorUsuarioId == usuarioId &&
                    turno.PacienteUsuarioId == registro.PacienteUsuarioId &&
                    (turno.Estado == TurnoEstados.Confirmado ||
                        turno.Estado == TurnoEstados.Reprogramado))),
            _ => query.Where(_ => false)
        };

        return pacienteUsuarioId.HasValue
            ? consulta.Where(registro => registro.PacienteUsuarioId == pacienteUsuarioId.Value)
            : consulta;
    }

    private Task<bool> DoctorAtiendePacienteAsync(
        int pacienteUsuarioId,
        CancellationToken cancellationToken) =>
        context.Turnos.AnyAsync(turno =>
            turno.DoctorUsuarioId == AuthClaims.ObtenerUsuarioId(User) &&
            turno.PacienteUsuarioId == pacienteUsuarioId &&
            (turno.Estado == TurnoEstados.Confirmado ||
                turno.Estado == TurnoEstados.Reprogramado),
            cancellationToken);

    private string? ObtenerRol() =>
        User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirst("role")?.Value;

    private async Task<bool> AsignacionValidaAsync(RegistroHistoriaClinicaRequest request, string? rol)
    {
        if (rol == "doctor")
        {
            var doctorId = AuthClaims.ObtenerUsuarioId(User);
            return request.PacienteUsuarioId.HasValue &&
                await context.Usuarios.AnyAsync(usuario =>
                    usuario.Id == request.PacienteUsuarioId &&
                    usuario.Activo &&
                    usuario.Rol != null &&
                    usuario.Rol.Codigo == "paciente") &&
                await context.Turnos.AnyAsync(turno =>
                    turno.DoctorUsuarioId == doctorId &&
                    turno.PacienteUsuarioId == request.PacienteUsuarioId &&
                    (turno.Estado == TurnoEstados.Confirmado ||
                        turno.Estado == TurnoEstados.Reprogramado));
        }

        return false;
    }

    private static void Aplicar(
        RegistroHistoriaClinicaRequest request,
        RegistroHistoriaClinica registro,
        bool mantenerAsignacionesSinCambio = false)
    {
        if (!mantenerAsignacionesSinCambio || request.PacienteUsuarioId.HasValue)
        {
            registro.PacienteUsuarioId = request.PacienteUsuarioId;
        }

        if (!mantenerAsignacionesSinCambio || request.DoctorUsuarioId.HasValue)
        {
            registro.DoctorUsuarioId = request.DoctorUsuarioId;
        }

        registro.Fecha = request.Fecha;
        registro.Tipo = request.Tipo.Trim();
        registro.Valor = request.Valor.Trim();
        registro.Notas = request.Notas.Trim();
    }

    private static RegistroHistoriaClinicaResumenDto Mapear(RegistroHistoriaClinica registro) =>
        new(
            registro.Id,
            registro.PacienteUsuarioId,
            registro.DoctorUsuarioId,
            registro.Fecha,
            registro.Tipo,
            registro.Valor,
            registro.Notas);

    private BadRequestObjectResult ProblemaValidacion(string detail) =>
        BadRequest(new { codigo = "validacion", mensaje = detail });

    private NotFoundObjectResult ProblemaNoEncontrado(string detail) =>
        NotFound(new { codigo = "recurso_no_encontrado", mensaje = detail });
}
