using Medicore.Api.Data;
using Medicore.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medicore.Api.Controllers;

[ApiController]
[Route("api/nutricion")]
public class NutricionController : ControllerBase
{
    private readonly MedicoreDbContext context;

    public NutricionController(MedicoreDbContext context)
    {
        this.context = context;
    }

    [HttpGet]
    public async Task<ActionResult<object>> ObtenerPlan()
    {
        var plan = await context.PlanNutricional
            .AsNoTracking()
            .OrderBy(item => item.Id)
            .Select(item => new ItemNutricionResumenDto(
                item.Id,
                item.Dia,
                item.Comida,
                item.Detalle
            ))
            .ToListAsync();

        return Ok(new { plan });
    }
}
