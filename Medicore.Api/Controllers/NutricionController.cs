using Medicore.Api.Data;
using Medicore.Api.Dtos;
using Medicore.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medicore.Api.Controllers;

[ApiController]
[Route("api/nutricion")]
[Authorize(Roles = "paciente,doctor")]
public class NutricionController : ControllerBase
{
    private readonly MedicoreDbContext context;

    public NutricionController(MedicoreDbContext context)
    {
        this.context = context;
    }

    [HttpGet]
    public async Task<ActionResult<object>> ObtenerPlan(
        [FromQuery] string? dia = null,
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

        var query = context.PlanNutricional
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(dia) && !string.Equals(dia, "Todos", StringComparison.OrdinalIgnoreCase))
        {
            var diaFiltrado = dia.Trim();
            query = query.Where(item => item.Dia == diaFiltrado);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var valor = busqueda.Trim();
            query = query.Where(item =>
                item.Dia.Contains(valor) ||
                item.Comida.Contains(valor) ||
                item.Detalle.Contains(valor));
        }

        var total = await query.CountAsync();
        var plan = await query
            .OrderBy(item => item.Dia)
            .ThenBy(item => item.Comida)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(item => new ItemNutricionResumenDto(
                item.Id, item.Dia, item.Comida, item.Detalle))
            .ToListAsync();

        var totalPaginas = total == 0 ? 0 : (int)Math.Ceiling(total / (double)tamano);

        return Ok(new
        {
            plan,
            pagina,
            tamano,
            total,
            totalPaginas,
            filtros = new
            {
                busqueda = busqueda ?? string.Empty,
                dia = dia ?? "Todos"
            }
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ItemNutricionResumenDto>> ObtenerItem(int id)
    {
        var item = await context.PlanNutricional
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new ItemNutricionResumenDto(
                item.Id, item.Dia, item.Comida, item.Detalle))
            .SingleOrDefaultAsync();

        return item is null
            ? ProblemaNoEncontrado("El ítem del plan nutricional solicitado no existe.")
            : Ok(item);
    }

    [Authorize(Roles = "doctor")]
    [HttpPost]
    public async Task<ActionResult<ItemNutricionResumenDto>> CrearItem(ItemNutricionRequest request)
    {
        var validacion = await ValidarItemAsync(request);
        if (validacion is not null)
        {
            return validacion;
        }

        var item = new ItemNutricion();
        Aplicar(request, item);

        context.PlanNutricional.Add(item);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerItem), new { id = item.Id }, Mapear(item));
    }

    [Authorize(Roles = "doctor")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ItemNutricionResumenDto>> ModificarItem(int id, ItemNutricionRequest request)
    {
        var item = await context.PlanNutricional.FindAsync(id);
        if (item is null)
        {
            return ProblemaNoEncontrado("El ítem del plan nutricional solicitado no existe.");
        }

        var validacion = await ValidarItemAsync(request, id);
        if (validacion is not null)
        {
            return validacion;
        }

        Aplicar(request, item);
        await context.SaveChangesAsync();

        return Ok(Mapear(item));
    }

    [Authorize(Roles = "doctor")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarItem(int id)
    {
        var item = await context.PlanNutricional.FindAsync(id);
        if (item is null)
        {
            return ProblemaNoEncontrado("El ítem del plan nutricional solicitado no existe.");
        }

        context.PlanNutricional.Remove(item);
        await context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<ActionResult<ItemNutricionResumenDto>?> ValidarItemAsync(ItemNutricionRequest request, int? id = null)
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

        var dia = request.Dia.Trim();
        var comida = request.Comida.Trim();
        var detalleValidacion = request.Detalle.Trim();

        if (string.IsNullOrWhiteSpace(dia))
        {
            return ProblemaValidacion("El día es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(comida))
        {
            return ProblemaValidacion("La comida es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(detalleValidacion))
        {
            return ProblemaValidacion("El detalle es obligatorio.");
        }

        var yaExiste = await context.PlanNutricional.AnyAsync(item =>
            item.Id != (id ?? 0) &&
            item.Dia == dia &&
            item.Comida == comida);

        if (yaExiste)
        {
            return ProblemaValidacion("Ya existe un ítem para ese día y comida.");
        }

        return null;
    }

    private static void Aplicar(ItemNutricionRequest request, ItemNutricion item)
    {
        item.Dia = request.Dia.Trim();
        item.Comida = request.Comida.Trim();
        item.Detalle = request.Detalle.Trim();
    }

    private static ItemNutricionResumenDto Mapear(ItemNutricion item) =>
        new(item.Id, item.Dia, item.Comida, item.Detalle);

    private BadRequestObjectResult ProblemaValidacion(string detail) =>
        BadRequest(new { codigo = "validacion", mensaje = detail });

    private NotFoundObjectResult ProblemaNoEncontrado(string detail) =>
        NotFound(new { codigo = "recurso_no_encontrado", mensaje = detail });
}
