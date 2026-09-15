using Medicore.Api.Data;
using Medicore.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medicore.Api.Controllers;

[ApiController]
[Route("api/historia-clinica")]
public class HistoriaClinicaController : ControllerBase
{
    private readonly MedicoreDbContext context;

    public HistoriaClinicaController(MedicoreDbContext context)
    {
        this.context = context;
    }

    [HttpGet]
    public async Task<ActionResult<object>> ObtenerRegistros()
    {
        var registros = await context.HistoriaClinica
            .AsNoTracking()
            .OrderByDescending(registro => registro.Fecha)
            .ThenByDescending(registro => registro.Id)
            .Select(registro => new RegistroHistoriaClinicaResumenDto(
                registro.Id,
                registro.Fecha,
                registro.Tipo,
                registro.Valor,
                registro.Notas
            ))
            .ToListAsync();

        return Ok(new { registros });
    }
}
