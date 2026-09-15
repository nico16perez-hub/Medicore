using Medicore.Api.Data;
using Medicore.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medicore.Api.Controllers;

[ApiController]
[Route("api/turnos")]
public class TurnosController : ControllerBase
{
    private readonly MedicoreDbContext context;

    public TurnosController(MedicoreDbContext context)
    {
        this.context = context;
    }

    [HttpGet]
    public async Task<ActionResult<object>> ObtenerTurnos()
    {
        var turnos = await context.Turnos
            .AsNoTracking()
            .OrderBy(turno => turno.Fecha)
            .ThenBy(turno => turno.Hora)
            .Select(turno => new TurnoResumenDto(
                turno.Id,
                turno.Profesional,
                turno.Especialidad,
                turno.Fecha,
                turno.Hora,
                turno.Estado
            ))
            .ToListAsync();

        return Ok(new { turnos });
    }
}
