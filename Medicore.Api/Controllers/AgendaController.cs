using System.Security.Claims;
using Medicore.Api.Data;
using Medicore.Api.Dtos;
using Medicore.Api.Models;
using Medicore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medicore.Api.Controllers;

[ApiController]
[Route("api/agenda")]
[Authorize(Roles = "secretaria")]
public class AgendaController(MedicoreDbContext context) : ControllerBase
{
    private const int DuracionTurnoMinutos = 30;
    private const int MaximoDiasAgenda = 60;

    [HttpGet("doctores")]
    public async Task<ActionResult<IReadOnlyList<DoctorAgendaDto>>> ObtenerDoctores(
        CancellationToken cancellationToken)
    {
        await AsegurarHorariosPredeterminadosAsync(cancellationToken);
        var doctores = await context.Usuarios
            .AsNoTracking()
            .Where(usuario => usuario.Activo &&
                usuario.Rol != null &&
                usuario.Rol.Activo &&
                usuario.Rol.Codigo == "doctor")
            .OrderBy(usuario => usuario.Email)
            .Select(usuario => new { usuario.Id, usuario.Email })
            .ToListAsync(cancellationToken);
        var doctorIds = doctores.Select(doctor => doctor.Id).ToArray();
        var especialidades = await context.DoctoresEspecialidades
            .AsNoTracking()
            .Where(item => doctorIds.Contains(item.DoctorUsuarioId))
            .Select(item => new { item.DoctorUsuarioId, item.Especialidad })
            .ToListAsync(cancellationToken);

        return Ok(doctores.Select(doctor => new DoctorAgendaDto(
            doctor.Id,
            doctor.Email,
            especialidades
                .Where(item => item.DoctorUsuarioId == doctor.Id)
                .Select(item => item.Especialidad)
                .OrderBy(item => item)
                .ToArray())));
    }

    [HttpGet("pacientes")]
    public async Task<ActionResult<IReadOnlyList<DoctorAgendaDto>>> ObtenerPacientes(
        CancellationToken cancellationToken)
    {
        var pacientes = await context.Usuarios
            .AsNoTracking()
            .Where(usuario => usuario.Activo &&
                usuario.Rol != null &&
                usuario.Rol.Activo &&
                usuario.Rol.Codigo == "paciente")
            .OrderBy(usuario => usuario.Email)
            .Select(usuario => new DoctorAgendaDto(usuario.Id, usuario.Email))
            .ToListAsync(cancellationToken);

        return Ok(pacientes);
    }

    [HttpGet("doctores/{doctorUsuarioId:int}/disponibilidad")]
    public async Task<ActionResult<IReadOnlyList<DisponibilidadDoctorRequest>>> ObtenerDisponibilidad(
        int doctorUsuarioId,
        CancellationToken cancellationToken)
    {
        if (!await DoctorActivoAsync(doctorUsuarioId, cancellationToken))
        {
            return NotFound(new
            {
                codigo = "doctor_no_encontrado",
                mensaje = "No se encontró un doctor activo."
            });
        }

        await AsegurarHorariosPredeterminadosAsync(cancellationToken);
        var disponibilidad = await context.DisponibilidadesDoctores
            .AsNoTracking()
            .Where(item => item.DoctorUsuarioId == doctorUsuarioId)
            .OrderBy(item => item.DiaSemana)
            .ThenBy(item => item.HoraInicio)
            .Select(item => new DisponibilidadDoctorRequest(
                (int)item.DiaSemana,
                item.HoraInicio,
                item.HoraFin))
            .ToListAsync(cancellationToken);

        return Ok(disponibilidad);
    }

    [HttpGet("doctores/{doctorUsuarioId:int}/especialidades")]
    public async Task<ActionResult<IReadOnlyList<string>>> ObtenerEspecialidades(
        int doctorUsuarioId,
        CancellationToken cancellationToken)
    {
        if (!await DoctorActivoAsync(doctorUsuarioId, cancellationToken))
        {
            return NotFound(new
            {
                codigo = "doctor_no_encontrado",
                mensaje = "No se encontró un doctor activo."
            });
        }

        var especialidades = await context.DoctoresEspecialidades
            .AsNoTracking()
            .Where(item => item.DoctorUsuarioId == doctorUsuarioId)
            .OrderBy(item => item.Especialidad)
            .Select(item => item.Especialidad)
            .ToListAsync(cancellationToken);
        return Ok(especialidades);
    }

    [HttpPut("doctores/{doctorUsuarioId:int}/especialidades")]
    public async Task<IActionResult> GuardarEspecialidades(
        int doctorUsuarioId,
        [FromBody] IReadOnlyList<string> especialidades,
        CancellationToken cancellationToken)
    {
        if (!await DoctorActivoAsync(doctorUsuarioId, cancellationToken))
        {
            return NotFound(new
            {
                codigo = "doctor_no_encontrado",
                mensaje = "No se encontró un doctor activo."
            });
        }

        if (especialidades is null ||
            especialidades.Any(item => string.IsNullOrWhiteSpace(item) || item.Trim().Length > 120))
        {
            return BadRequest(new
            {
                codigo = "especialidades_invalidas",
                mensaje = "Cada especialidad debe tener entre 1 y 120 caracteres."
            });
        }

        var nuevasEspecialidades = especialidades
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var turnosVigentes = await context.Turnos
            .AsNoTracking()
            .Where(turno => (turno.DoctorUsuarioId == doctorUsuarioId &&
                turno.Fecha >= DateOnly.FromDateTime(DateTime.Today) &&
                (turno.Estado == TurnoEstados.Confirmado ||
                    turno.Estado == TurnoEstados.Reprogramado)) ||
                (turno.DoctorPreferidoUsuarioId == doctorUsuarioId &&
                    turno.Fecha >= DateOnly.FromDateTime(DateTime.Today) &&
                    turno.Estado == TurnoEstados.Pendiente))
            .Select(turno => turno.Especialidad)
            .Distinct()
            .ToListAsync(cancellationToken);
        if (turnosVigentes.Any(especialidad =>
                !nuevasEspecialidades.Contains(especialidad, StringComparer.OrdinalIgnoreCase)))
        {
            return BadRequest(new
            {
                codigo = "especialidad_con_turnos_asignados",
                mensaje = "No se puede quitar un área que tiene turnos vigentes asignados a este doctor."
            });
        }

        var actuales = await context.DoctoresEspecialidades
            .Where(item => item.DoctorUsuarioId == doctorUsuarioId)
            .ToListAsync(cancellationToken);
        var especialidadesDeseadas = nuevasEspecialidades.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var especialidadesActuales = actuales
            .Select(item => item.Especialidad)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        context.DoctoresEspecialidades.RemoveRange(actuales.Where(item =>
            !especialidadesDeseadas.Contains(item.Especialidad)));
        context.DoctoresEspecialidades.AddRange(nuevasEspecialidades
            .Where(especialidad => !especialidadesActuales.Contains(especialidad))
            .Select(especialidad =>
            new DoctorEspecialidad
            {
                DoctorUsuarioId = doctorUsuarioId,
                Especialidad = especialidad
            }));
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPut("doctores/{doctorUsuarioId:int}/disponibilidad")]
    public async Task<IActionResult> GuardarDisponibilidad(
        int doctorUsuarioId,
        [FromBody] IReadOnlyList<DisponibilidadDoctorRequest> disponibilidad,
        CancellationToken cancellationToken)
    {
        if (!await DoctorActivoAsync(doctorUsuarioId, cancellationToken))
        {
            return NotFound(new
            {
                codigo = "doctor_no_encontrado",
                mensaje = "No se encontró un doctor activo."
            });
        }

        var error = ValidarDisponibilidad(disponibilidad);
        if (error is not null)
        {
            return BadRequest(new { codigo = "disponibilidad_invalida", mensaje = error });
        }

        var nuevaDisponibilidad = disponibilidad
            .Select(item => new DisponibilidadDoctor
            {
                DoctorUsuarioId = doctorUsuarioId,
                DiaSemana = (DayOfWeek)item.DiaSemana,
                HoraInicio = item.HoraInicio,
                HoraFin = item.HoraFin
            })
            .ToList();

        var turnosFuturos = await context.Turnos
            .AsNoTracking()
            .Where(turno => turno.DoctorUsuarioId == doctorUsuarioId &&
                turno.Fecha >= DateOnly.FromDateTime(DateTime.Today) &&
                (turno.Estado == TurnoEstados.Confirmado ||
                    turno.Estado == TurnoEstados.Reprogramado))
            .Select(turno => new { turno.Fecha, turno.Hora })
            .ToListAsync(cancellationToken);

        if (turnosFuturos.Any(turno => !EstaDentroDeDisponibilidad(
                turno.Fecha, turno.Hora, nuevaDisponibilidad)))
        {
            return BadRequest(new
            {
                codigo = "horario_con_turnos_asignados",
                mensaje = "No se puede quitar un horario que contiene turnos vigentes. Reprogramá esos turnos antes de cambiar la agenda."
            });
        }

        var actual = await context.DisponibilidadesDoctores
            .Where(item => item.DoctorUsuarioId == doctorUsuarioId)
            .ToListAsync(cancellationToken);
        await using var transaccion = await context.Database.BeginTransactionAsync(cancellationToken);
        context.DisponibilidadesDoctores.RemoveRange(actual);
        await context.SaveChangesAsync(cancellationToken);
        context.DisponibilidadesDoctores.AddRange(nuevaDisponibilidad);
        await context.SaveChangesAsync(cancellationToken);
        await transaccion.CommitAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("disponibles")]
    public async Task<ActionResult<IReadOnlyList<SlotDisponibleDto>>> ObtenerHorariosDisponibles(
        [FromQuery] int doctorUsuarioId,
        [FromQuery] DateOnly? desde,
        [FromQuery] int dias = 28,
        CancellationToken cancellationToken = default)
    {
        if (doctorUsuarioId < 1 || !await DoctorActivoAsync(doctorUsuarioId, cancellationToken))
        {
            return BadRequest(new
            {
                codigo = "doctor_invalido",
                mensaje = "Seleccioná un doctor activo."
            });
        }

        if (dias < 1 || dias > MaximoDiasAgenda)
        {
            return BadRequest(new
            {
                codigo = "rango_invalido",
                mensaje = $"La agenda debe consultarse entre 1 y {MaximoDiasAgenda} días."
            });
        }

        var fechaInicial = desde ?? DateOnly.FromDateTime(DateTime.Today);
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        if (fechaInicial < hoy || fechaInicial > hoy.AddDays(MaximoDiasAgenda))
        {
            return BadRequest(new
            {
                codigo = "fecha_invalida",
                mensaje = "La agenda solo puede consultarse desde hoy y hasta 60 días hacia adelante."
            });
        }

        await AsegurarHorariosPredeterminadosAsync(cancellationToken);
        var fechaFinal = fechaInicial.AddDays(dias);
        var disponibilidad = await context.DisponibilidadesDoctores
            .AsNoTracking()
            .Where(item => item.DoctorUsuarioId == doctorUsuarioId)
            .ToListAsync(cancellationToken);
        var ocupados = await context.Turnos
            .AsNoTracking()
            .Where(turno => turno.DoctorUsuarioId == doctorUsuarioId &&
                turno.Fecha >= fechaInicial &&
                turno.Fecha < fechaFinal &&
                (turno.Estado == TurnoEstados.Confirmado ||
                    turno.Estado == TurnoEstados.Reprogramado))
            .Select(turno => new { turno.Fecha, turno.Hora })
            .ToListAsync(cancellationToken);

        var ahora = TimeOnly.FromDateTime(DateTime.Now);
        var horarios = new List<SlotDisponibleDto>();
        for (var fecha = fechaInicial; fecha < fechaFinal; fecha = fecha.AddDays(1))
        {
            foreach (var rango in disponibilidad.Where(item => item.DiaSemana == fecha.DayOfWeek))
            {
                for (var hora = rango.HoraInicio;
                     hora.AddMinutes(DuracionTurnoMinutos) <= rango.HoraFin;
                     hora = hora.AddMinutes(DuracionTurnoMinutos))
                {
                    if (fecha == hoy && hora <= ahora ||
                        ocupados.Any(turno => turno.Fecha == fecha &&
                            turno.Hora < hora.AddMinutes(DuracionTurnoMinutos) &&
                            turno.Hora.AddMinutes(DuracionTurnoMinutos) > hora))
                    {
                        continue;
                    }

                    horarios.Add(new SlotDisponibleDto(fecha, hora));
                }
            }
        }

        return Ok(horarios.OrderBy(item => item.Fecha).ThenBy(item => item.Hora));
    }

    private async Task<bool> DoctorActivoAsync(int doctorUsuarioId, CancellationToken cancellationToken) =>
        await context.Usuarios.AnyAsync(usuario =>
            usuario.Id == doctorUsuarioId &&
            usuario.Activo &&
            usuario.Rol != null &&
            usuario.Rol.Activo &&
            usuario.Rol.Codigo == "doctor",
            cancellationToken);

    private async Task AsegurarHorariosPredeterminadosAsync(CancellationToken cancellationToken)
    {
        var doctores = await context.Usuarios
            .AsNoTracking()
            .Where(usuario => usuario.Activo &&
                usuario.Rol != null &&
                usuario.Rol.Activo &&
                usuario.Rol.Codigo == "doctor")
            .Select(usuario => usuario.Id)
            .ToListAsync(cancellationToken);
        if (doctores.Count == 0)
        {
            return;
        }

        var disponibilidad = await context.DisponibilidadesDoctores
            .Where(item => doctores.Contains(item.DoctorUsuarioId))
            .ToListAsync(cancellationToken);
        var predeterminados = new List<DisponibilidadDoctor>();
        foreach (var doctorId in doctores)
        {
            if (disponibilidad.Any(item => item.DoctorUsuarioId == doctorId))
            {
                continue;
            }

            foreach (var dia in new[]
                     {
                         DayOfWeek.Monday,
                         DayOfWeek.Tuesday,
                         DayOfWeek.Wednesday,
                         DayOfWeek.Thursday,
                         DayOfWeek.Friday
                     })
            {
                if (!disponibilidad.Any(item =>
                        item.DoctorUsuarioId == doctorId && item.DiaSemana == dia))
                {
                    predeterminados.Add(new DisponibilidadDoctor
                    {
                        DoctorUsuarioId = doctorId,
                        DiaSemana = dia,
                        HoraInicio = new TimeOnly(8, 0),
                        HoraFin = new TimeOnly(13, 0)
                    });
                }
            }
        }

        if (predeterminados.Count > 0)
        {
            context.DisponibilidadesDoctores.AddRange(predeterminados);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static string? ValidarDisponibilidad(
        IReadOnlyList<DisponibilidadDoctorRequest> disponibilidad)
    {
        foreach (var rango in disponibilidad)
        {
            if (rango.DiaSemana is < 0 or > 6)
            {
                return "El día de la semana no es válido.";
            }

            if (rango.HoraInicio >= rango.HoraFin ||
                rango.HoraInicio.Minute % DuracionTurnoMinutos != 0 ||
                rango.HoraFin.Minute % DuracionTurnoMinutos != 0 ||
                rango.HoraInicio.Second != 0 ||
                rango.HoraFin.Second != 0 ||
                (rango.HoraFin - rango.HoraInicio).TotalMinutes % DuracionTurnoMinutos != 0)
            {
                return "Los horarios deben ser rangos válidos en bloques de 30 minutos.";
            }
        }

        var rangosOrdenados = disponibilidad
            .OrderBy(item => item.DiaSemana)
            .ThenBy(item => item.HoraInicio)
            .ToList();
        for (var indice = 1; indice < rangosOrdenados.Count; indice++)
        {
            var anterior = rangosOrdenados[indice - 1];
            var actual = rangosOrdenados[indice];
            if (anterior.DiaSemana == actual.DiaSemana &&
                actual.HoraInicio < anterior.HoraFin)
            {
                return "Los rangos horarios de un mismo día no pueden superponerse.";
            }
        }

        return null;
    }

    private static bool EstaDentroDeDisponibilidad(
        DateOnly fecha,
        TimeOnly hora,
        IEnumerable<DisponibilidadDoctor> disponibilidad) =>
        disponibilidad.Any(rango =>
            rango.DiaSemana == fecha.DayOfWeek &&
            hora >= rango.HoraInicio &&
            hora.AddMinutes(DuracionTurnoMinutos) <= rango.HoraFin);
}
