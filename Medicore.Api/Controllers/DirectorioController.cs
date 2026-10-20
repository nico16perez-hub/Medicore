using Medicore.Api.Data;
using Medicore.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medicore.Api.Controllers;

[ApiController]
[Route("api/directorio")]
[Authorize(Roles = "paciente,secretaria")]
public class DirectorioController(MedicoreDbContext context) : ControllerBase
{
    [HttpGet("doctores")]
    public async Task<ActionResult<IReadOnlyList<DoctorAgendaDto>>> ObtenerDoctores(
        CancellationToken cancellationToken)
    {
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
            .OrderBy(item => item.Especialidad)
            .Select(item => new { item.DoctorUsuarioId, item.Especialidad })
            .ToListAsync(cancellationToken);

        return Ok(doctores.Select(doctor => new DoctorAgendaDto(
            doctor.Id,
            doctor.Email,
            especialidades
                .Where(item => item.DoctorUsuarioId == doctor.Id)
                .Select(item => item.Especialidad)
                .ToArray())));
    }
}
