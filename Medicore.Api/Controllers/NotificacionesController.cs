using Medicore.Api.Data;
using Medicore.Api.Dtos;
using Medicore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medicore.Api.Controllers;

[ApiController]
[Route("api/notificaciones")]
[Authorize]
public sealed class NotificacionesController(MedicoreDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObtenerMisNotificaciones(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(1, pagina);
        tamano = Math.Clamp(tamano, 1, 50);
        var usuarioId = AuthClaims.ObtenerUsuarioId(User);
        var consulta = context.Notificaciones
            .AsNoTracking()
            .Where(notificacion => notificacion.UsuarioId == usuarioId);
        var total = await consulta.CountAsync(cancellationToken);
        var notificaciones = await consulta
            .OrderByDescending(notificacion => notificacion.CreadaEnUtc)
            .ThenByDescending(notificacion => notificacion.Id)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(notificacion => new NotificacionDto(
                notificacion.Id,
                notificacion.Titulo,
                notificacion.Mensaje,
                notificacion.CreadaEnUtc,
                notificacion.Leida,
                notificacion.TurnoId))
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            notificaciones,
            pagina,
            tamano,
            total,
            totalPaginas = total == 0 ? 0 : (int)Math.Ceiling(total / (double)tamano)
        });
    }

    [HttpGet("no-leidas")]
    public async Task<IActionResult> ObtenerCantidadNoLeidas(CancellationToken cancellationToken)
    {
        var usuarioId = AuthClaims.ObtenerUsuarioId(User);
        var total = await context.Notificaciones.CountAsync(
            notificacion => notificacion.UsuarioId == usuarioId && !notificacion.Leida,
            cancellationToken);
        return Ok(new { total });
    }

    [HttpPut("{id:int}/leida")]
    public async Task<IActionResult> MarcarComoLeida(int id, CancellationToken cancellationToken)
    {
        var usuarioId = AuthClaims.ObtenerUsuarioId(User);
        var notificacion = await context.Notificaciones.SingleOrDefaultAsync(
            item => item.Id == id && item.UsuarioId == usuarioId,
            cancellationToken);

        if (notificacion is null)
        {
            return NotFound(new
            {
                codigo = "notificacion_no_encontrada",
                mensaje = "No se encontró la notificación solicitada."
            });
        }

        if (!notificacion.Leida)
        {
            notificacion.Leida = true;
            await context.SaveChangesAsync(cancellationToken);
        }

        return NoContent();
    }

    [HttpPut("leidas")]
    public async Task<IActionResult> MarcarTodasComoLeidas(CancellationToken cancellationToken)
    {
        var usuarioId = AuthClaims.ObtenerUsuarioId(User);
        await context.Notificaciones
            .Where(item => item.UsuarioId == usuarioId && !item.Leida)
            .ExecuteUpdateAsync(actualizacion => actualizacion
                .SetProperty(item => item.Leida, true), cancellationToken);
        return NoContent();
    }
}
