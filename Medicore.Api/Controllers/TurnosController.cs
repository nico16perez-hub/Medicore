using Medicore.Api.Data;
using Medicore.Api.Dtos;
using Medicore.Api.Models;
using Medicore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using System.Security.Claims;

namespace Medicore.Api.Controllers;

[ApiController]
[Route("api/turnos")]
[Authorize(Roles = "paciente,doctor,secretaria")]
public class TurnosController : ControllerBase
{
    private static readonly HashSet<string> EstadosValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        TurnoEstados.Pendiente,
        TurnoEstados.Confirmado,
        TurnoEstados.Reprogramado,
        TurnoEstados.Cancelado
    };

    private readonly MedicoreDbContext context;
    private readonly NotificacionesTurnosService notificaciones;

    public TurnosController(MedicoreDbContext context, NotificacionesTurnosService notificaciones)
    {
        this.context = context;
        this.notificaciones = notificaciones;
    }

    [HttpGet]
    public async Task<ActionResult<object>> ObtenerTurnos(
        [FromQuery] string? estado = null,
        [FromQuery] string? busqueda = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 5)
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

        var query = context.Turnos
            .AsNoTracking()
            .AsQueryable();
        var usuarioId = AuthClaims.ObtenerUsuarioId(User);
        var rol = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirst("role")?.Value;
        if (rol == "paciente")
        {
            query = query.Where(turno => turno.PacienteUsuarioId == usuarioId);
        }
        else if (rol == "doctor")
        {
            query = query.Where(turno => turno.DoctorUsuarioId == usuarioId);
        }

        if (!string.IsNullOrWhiteSpace(estado) && !string.Equals(estado, "Todos", StringComparison.OrdinalIgnoreCase))
        {
            var estadoFiltrado = estado.Trim();
            query = query.Where(turno => turno.Estado == estadoFiltrado);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var valor = busqueda.Trim();
            query = query.Where(turno =>
                turno.Profesional.Contains(valor) ||
                turno.Especialidad.Contains(valor) ||
                turno.Estado.Contains(valor));
        }

        var total = await query.CountAsync();
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var turnosOrdenados = rol == "secretaria"
            ? query.OrderBy(turno => turno.Estado == TurnoEstados.Pendiente ? 0 : 1)
                .ThenBy(turno => turno.Fecha)
                .ThenBy(turno => turno.Hora)
            : query.OrderBy(turno => turno.Estado == TurnoEstados.Cancelado)
                .ThenBy(turno => turno.Fecha < hoy)
                .ThenBy(turno => turno.Fecha)
                .ThenBy(turno => turno.Hora);
        var turnos = await turnosOrdenados
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(turno => new TurnoResumenDto(
                turno.Id, turno.PacienteUsuarioId, turno.DoctorUsuarioId,
                turno.Profesional, turno.Especialidad,
                turno.Fecha, turno.Hora, turno.Estado,
                turno.PacienteUsuarioId.HasValue
                    ? context.Usuarios.Where(usuario => usuario.Id == turno.PacienteUsuarioId.Value)
                        .Select(usuario => usuario.Email).FirstOrDefault()
                    : null,
                turno.DoctorUsuarioId.HasValue
                    ? context.Usuarios.Where(usuario => usuario.Id == turno.DoctorUsuarioId.Value)
                        .Select(usuario => usuario.Email).FirstOrDefault()
                    : null,
                turno.DoctorPreferidoUsuarioId,
                turno.DoctorPreferidoUsuarioId.HasValue
                    ? context.Usuarios.Where(usuario => usuario.Id == turno.DoctorPreferidoUsuarioId.Value)
                        .Select(usuario => usuario.Email).FirstOrDefault()
                    : null))
            .ToListAsync();

        var totalPaginas = total == 0 ? 0 : (int)Math.Ceiling(total / (double)tamano);

        return Ok(new
        {
            turnos,
            pagina,
            tamano,
            total,
            totalPaginas,
            filtros = new
            {
                busqueda = busqueda ?? string.Empty,
                estado = estado ?? "Todos"
            }
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TurnoResumenDto>> ObtenerTurno(int id)
    {
        var consulta = context.Turnos
            .AsNoTracking()
            .Where(turno => turno.Id == id);
        var usuarioId = AuthClaims.ObtenerUsuarioId(User);
        var rol = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirst("role")?.Value;
        if (rol == "paciente")
        {
            consulta = consulta.Where(turno => turno.PacienteUsuarioId == usuarioId);
        }
        else if (rol == "doctor")
        {
            consulta = consulta.Where(turno => turno.DoctorUsuarioId == usuarioId);
        }

        var turno = await consulta
            .Select(turno => new TurnoResumenDto(
                turno.Id, turno.PacienteUsuarioId, turno.DoctorUsuarioId,
                turno.Profesional, turno.Especialidad,
                turno.Fecha, turno.Hora, turno.Estado,
                turno.PacienteUsuarioId.HasValue
                    ? context.Usuarios.Where(usuario => usuario.Id == turno.PacienteUsuarioId.Value)
                        .Select(usuario => usuario.Email).FirstOrDefault()
                    : null,
                turno.DoctorUsuarioId.HasValue
                    ? context.Usuarios.Where(usuario => usuario.Id == turno.DoctorUsuarioId.Value)
                        .Select(usuario => usuario.Email).FirstOrDefault()
                    : null,
                turno.DoctorPreferidoUsuarioId,
                turno.DoctorPreferidoUsuarioId.HasValue
                    ? context.Usuarios.Where(usuario => usuario.Id == turno.DoctorPreferidoUsuarioId.Value)
                        .Select(usuario => usuario.Email).FirstOrDefault()
                    : null))
            .SingleOrDefaultAsync();

        return turno is null
            ? ProblemaNoEncontrado("El turno solicitado no existe.")
            : Ok(turno);
    }

    [HttpGet("{id:int}/comprobante")]
    public async Task<IActionResult> ObtenerComprobante(int id, CancellationToken cancellationToken)
    {
        var usuarioId = AuthClaims.ObtenerUsuarioId(User);
        var rol = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirst("role")?.Value;
        var consulta = context.Turnos
            .AsNoTracking()
            .Where(turno => turno.Id == id);

        if (rol == "paciente")
        {
            consulta = consulta.Where(turno => turno.PacienteUsuarioId == usuarioId);
        }
        else if (rol == "doctor")
        {
            consulta = consulta.Where(turno => turno.DoctorUsuarioId == usuarioId);
        }

        var turno = await consulta.SingleOrDefaultAsync(cancellationToken);
        if (turno is null)
        {
            return ProblemaNoEncontrado("El turno solicitado no existe.");
        }

        if (!string.Equals(turno.Estado, TurnoEstados.Confirmado, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                codigo = "turno_no_confirmado",
                mensaje = "El comprobante solo está disponible para turnos confirmados."
            });
        }

        var paciente = turno.PacienteUsuarioId.HasValue
            ? await context.Usuarios
                .AsNoTracking()
                .Where(usuario => usuario.Id == turno.PacienteUsuarioId.Value)
                .Select(usuario => usuario.Email)
                .SingleOrDefaultAsync(cancellationToken)
            : null;
        var identificacionPaciente = paciente ?? (turno.PacienteUsuarioId.HasValue
            ? $"Usuario #{turno.PacienteUsuarioId.Value}"
            : "Paciente sin asignar");
        var contenidoQr = TurnoQr.CrearContenido(turno.Id);

        using var generador = new QRCodeGenerator();
        using var datosQr = generador.CreateQrCode(contenidoQr, QRCodeGenerator.ECCLevel.Q);
        var imagenQr = new PngByteQRCode(datosQr).GetGraphic(8);
        var pdf = Document.Create(documento =>
        {
            documento.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(42);
                pagina.DefaultTextStyle(estilo => estilo.FontSize(12).FontColor(Colors.Grey.Darken3));

                pagina.Content().Column(columna =>
                {
                    columna.Spacing(12);
                    columna.Item().Text("MediCore").FontSize(24).Bold().FontColor(Colors.Cyan.Darken2);
                    columna.Item().Text("Comprobante de turno").FontSize(18).Bold();
                    columna.Item().Text($"Código del turno: {turno.Id}").Bold();
                    columna.Item().Text($"Paciente: {identificacionPaciente}");
                    columna.Item().Text($"Especialidad: {turno.Especialidad}");
                    columna.Item().Text($"Profesional: {turno.Profesional}");
                    columna.Item().Text($"Fecha: {turno.Fecha:dd/MM/yyyy}");
                    columna.Item().Text($"Hora: {turno.Hora:HH\\:mm}");
                    columna.Item().Text($"Estado: {turno.Estado}");
                    columna.Item().PaddingTop(12).Width(150).Height(150).Image(imagenQr).FitArea();
                    columna.Item().Text("El código QR identifica únicamente este turno.");
                });
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf", $"comprobante-turno-{turno.Id}.pdf");
    }

    [Authorize(Roles = "paciente,secretaria")]
    [HttpPost]
    public async Task<ActionResult<TurnoResumenDto>> CrearTurno(TurnoRequest request)
    {
        var rol = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirst("role")?.Value;
        if (rol == "paciente" && request.Fecha < DateOnly.FromDateTime(DateTime.Today))
        {
            return ProblemaValidacion("La fecha preferida no puede estar en el pasado.");
        }

        if (rol == "secretaria" && !await AsignacionesValidasAsync(request))
        {
            return ProblemaValidacion("El paciente o doctor asignado no existe, está inactivo o no tiene el rol correspondiente.");
        }

        var validacion = await ValidarTurnoAsync(request);
        if (validacion is not null)
        {
            return validacion;
        }

        var turno = new Turno();
        Aplicar(request, turno);
        if (rol == "paciente")
        {
            turno.PacienteUsuarioId = AuthClaims.ObtenerUsuarioId(User);
            turno.DoctorUsuarioId = null;
            turno.DoctorPreferidoUsuarioId = request.DoctorPreferidoUsuarioId;
            turno.Estado = TurnoEstados.Pendiente;
        }

        context.Turnos.Add(turno);
        await notificaciones.AgregarNotificacionesSolicitudAsync(
            turno,
            AuthClaims.ObtenerUsuarioId(User),
            rol);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerTurno), new { id = turno.Id }, Mapear(turno));
    }

    [Authorize(Roles = "secretaria")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TurnoResumenDto>> ModificarTurno(int id, TurnoRequest request)
    {
        var turno = await context.Turnos.FindAsync(id);
        if (turno is null)
        {
            return ProblemaNoEncontrado("El turno solicitado no existe.");
        }

        if (!await AsignacionesValidasAsync(request))
        {
            return ProblemaValidacion("El paciente o doctor asignado no existe, está inactivo o no tiene el rol correspondiente.");
        }

        var validacion = await ValidarTurnoAsync(request, id);
        if (validacion is not null)
        {
            return validacion;
        }

        var estadoAnterior = turno.Estado;
        var fechaAnterior = turno.Fecha;
        var horaAnterior = turno.Hora;
        var profesionalAnterior = turno.Profesional;
        var especialidadAnterior = turno.Especialidad;
        var pacienteAnterior = turno.PacienteUsuarioId;
        var doctorAnterior = turno.DoctorUsuarioId;
        var doctorPreferidoAnterior = turno.DoctorPreferidoUsuarioId;
        Aplicar(request, turno, mantenerAsignacionesSinCambio: true);
        notificaciones.AgregarNotificacionesCambio(
            turno,
            AuthClaims.ObtenerUsuarioId(User),
            estadoAnterior,
            fechaAnterior,
            horaAnterior,
            profesionalAnterior,
            especialidadAnterior,
            pacienteAnterior,
            doctorAnterior,
            doctorPreferidoAnterior);
        await context.SaveChangesAsync();

        return Ok(Mapear(turno));
    }

    [Authorize(Roles = "secretaria")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarTurno(int id)
    {
        var turno = await context.Turnos.FindAsync(id);
        if (turno is null)
        {
            return ProblemaNoEncontrado("El turno solicitado no existe.");
        }

        if (string.Equals(turno.Estado, TurnoEstados.Cancelado, StringComparison.OrdinalIgnoreCase))
        {
            return NoContent();
        }

        var estadoAnterior = turno.Estado;
        turno.Estado = TurnoEstados.Cancelado;
        notificaciones.AgregarNotificacionesCambio(
            turno,
            AuthClaims.ObtenerUsuarioId(User),
            estadoAnterior,
            turno.Fecha,
            turno.Hora,
            turno.Profesional,
            turno.Especialidad,
            turno.PacienteUsuarioId,
            turno.DoctorUsuarioId,
            turno.DoctorPreferidoUsuarioId);
        await context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<ActionResult<TurnoResumenDto>?> ValidarTurnoAsync(TurnoRequest request, int? id = null)
    {
        var rol = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirst("role")?.Value;
        if (!ModelState.IsValid)
        {
            var detalle = string.Join(" ", ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "El valor enviado no es válido."
                    : error.ErrorMessage));

            return ProblemaValidacion(detalle);
        }

        var profesional = request.Profesional.Trim();
        var especialidad = request.Especialidad.Trim();
        var estado = request.Estado.Trim();

        if (string.IsNullOrWhiteSpace(profesional))
        {
            return ProblemaValidacion("El profesional es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(especialidad))
        {
            return ProblemaValidacion("La especialidad es obligatoria.");
        }

        if (rol == "paciente" &&
            (!request.DoctorPreferidoUsuarioId.HasValue ||
                !await DoctorTieneEspecialidadAsync(
                    request.DoctorPreferidoUsuarioId.Value,
                    especialidad)))
        {
            return ProblemaValidacion("Elegí un doctor activo que atienda en el área seleccionada.");
        }

        if (request.Fecha == default)
        {
            return ProblemaValidacion("La fecha del turno es obligatoria.");
        }

        if (request.Hora == default)
        {
            return ProblemaValidacion("La hora del turno es obligatoria.");
        }

        if (!EstadosValidos.Contains(estado))
        {
            return ProblemaValidacion("El estado del turno no es válido. Los valores permitidos son: Pendiente, Confirmado, Reprogramado, Cancelado.");
        }

        var requiereAsignacion = estado.Equals(TurnoEstados.Confirmado, StringComparison.OrdinalIgnoreCase) ||
            estado.Equals(TurnoEstados.Reprogramado, StringComparison.OrdinalIgnoreCase);
        if (requiereAsignacion && !request.DoctorUsuarioId.HasValue)
        {
            return ProblemaValidacion("Asigná un doctor antes de confirmar o reprogramar el turno.");
        }

        if (requiereAsignacion)
        {
            var turnoAnterior = id.HasValue
                ? await context.Turnos.AsNoTracking()
                    .Where(turno => turno.Id == id.Value)
                    .Select(turno => new
                    {
                        turno.DoctorUsuarioId,
                        turno.Fecha,
                        turno.Hora,
                        turno.Estado
                    })
                    .SingleOrDefaultAsync()
                : null;
            var nuevaAsignacion = turnoAnterior is null ||
                turnoAnterior.DoctorUsuarioId != request.DoctorUsuarioId ||
                turnoAnterior.Fecha != request.Fecha ||
                turnoAnterior.Hora != request.Hora ||
                turnoAnterior.Estado == TurnoEstados.Pendiente;

            if (nuevaAsignacion)
            {
                var doctorUsuarioId = request.DoctorUsuarioId.GetValueOrDefault();
                if (!await DoctorTieneEspecialidadAsync(doctorUsuarioId, especialidad))
                {
                    return ProblemaValidacion("El doctor seleccionado no atiende en el área del turno.");
                }

                var estaEnAgenda = await context.DisponibilidadesDoctores.AnyAsync(item =>
                    item.DoctorUsuarioId == request.DoctorUsuarioId &&
                    item.DiaSemana == request.Fecha.DayOfWeek &&
                    item.HoraInicio <= request.Hora &&
                    item.HoraFin >= request.Hora.AddMinutes(30));
                if (!estaEnAgenda)
                {
                    return ProblemaValidacion("El horario seleccionado no está dentro de la agenda semanal del doctor.");
                }

                var horariosOcupados = await context.Turnos
                    .AsNoTracking()
                    .Where(turno => turno.Id != (id ?? 0) &&
                        turno.DoctorUsuarioId == request.DoctorUsuarioId &&
                        turno.Fecha == request.Fecha &&
                        (turno.Estado == TurnoEstados.Confirmado ||
                            turno.Estado == TurnoEstados.Reprogramado))
                    .Select(turno => turno.Hora)
                    .ToListAsync();
                if (horariosOcupados.Any(hora =>
                    hora < request.Hora.AddMinutes(30) &&
                    hora.AddMinutes(30) > request.Hora))
                {
                    return ProblemaValidacion("Ese horario ya fue asignado a otro paciente.");
                }
            }
        }

        return null;
    }

    private async Task<bool> AsignacionesValidasAsync(TurnoRequest request)
    {
        var pacienteValido = !request.PacienteUsuarioId.HasValue ||
            await context.Usuarios.AnyAsync(usuario =>
                usuario.Id == request.PacienteUsuarioId &&
                usuario.Activo &&
                usuario.Rol != null &&
                usuario.Rol.Codigo == "paciente");
        var doctorValido = !request.DoctorUsuarioId.HasValue ||
            await context.Usuarios.AnyAsync(usuario =>
                usuario.Id == request.DoctorUsuarioId &&
                usuario.Activo &&
                usuario.Rol != null &&
                usuario.Rol.Activo &&
                usuario.Rol.Codigo == "doctor");
        var doctorPreferidoValido = !request.DoctorPreferidoUsuarioId.HasValue ||
            await DoctorTieneEspecialidadAsync(
                request.DoctorPreferidoUsuarioId.Value,
                request.Especialidad.Trim());
        return pacienteValido && doctorValido && doctorPreferidoValido;
    }

    private Task<bool> DoctorTieneEspecialidadAsync(int doctorUsuarioId, string especialidad) =>
        context.Usuarios.AnyAsync(usuario =>
            usuario.Id == doctorUsuarioId &&
            usuario.Activo &&
            usuario.Rol != null &&
            usuario.Rol.Activo &&
            usuario.Rol.Codigo == "doctor" &&
            context.DoctoresEspecialidades.Any(item =>
                item.DoctorUsuarioId == doctorUsuarioId &&
                item.Especialidad == especialidad));

    private static void Aplicar(
        TurnoRequest request,
        Turno turno,
        bool mantenerAsignacionesSinCambio = false)
    {
        if (!mantenerAsignacionesSinCambio || request.PacienteUsuarioId.HasValue)
        {
            turno.PacienteUsuarioId = request.PacienteUsuarioId;
        }

        if (!mantenerAsignacionesSinCambio || request.DoctorUsuarioId.HasValue)
        {
            turno.DoctorUsuarioId = request.DoctorUsuarioId;
        }

        if (!mantenerAsignacionesSinCambio || request.DoctorPreferidoUsuarioId.HasValue)
        {
            turno.DoctorPreferidoUsuarioId = request.DoctorPreferidoUsuarioId;
        }

        turno.Profesional = request.Profesional.Trim();
        turno.Especialidad = request.Especialidad.Trim();
        turno.Fecha = request.Fecha;
        turno.Hora = request.Hora;
        turno.Estado = request.Estado.Trim();
    }

    private static TurnoResumenDto Mapear(Turno turno) =>
        new(
            turno.Id,
            turno.PacienteUsuarioId,
            turno.DoctorUsuarioId,
            turno.Profesional,
            turno.Especialidad,
            turno.Fecha,
            turno.Hora,
            turno.Estado,
            DoctorPreferidoUsuarioId: turno.DoctorPreferidoUsuarioId);

    private BadRequestObjectResult ProblemaValidacion(string detail) =>
        BadRequest(new { codigo = "validacion", mensaje = detail });

    private NotFoundObjectResult ProblemaNoEncontrado(string detail) =>
        NotFound(new { codigo = "recurso_no_encontrado", mensaje = detail });
}
