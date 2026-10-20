using System.Security.Cryptography;
using System.Text;
using Medicore.Api.Data;
using Medicore.Api.Dtos;
using Medicore.Api.Models;
using Medicore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medicore.Api.Controllers;

[ApiController]
[Route("api/push")]
[Authorize]
public sealed class PushController(MedicoreDbContext context) : ControllerBase
{
    [HttpPost("dispositivos")]
    public async Task<IActionResult> RegistrarDispositivo(
        DispositivoPushRequest request,
        CancellationToken cancellationToken)
    {
        var plataforma = request.Plataforma.Trim().ToLowerInvariant();
        if (plataforma != "android")
        {
            return BadRequest(new
            {
                codigo = "plataforma_no_admitida",
                mensaje = "El registro push está preparado para Android."
            });
        }

        var token = request.Token.Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new
            {
                codigo = "token_requerido",
                mensaje = "El token push no puede estar vacío."
            });
        }

        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        var dispositivo = await context.DispositivosPush
            .SingleOrDefaultAsync(item => item.TokenHash == tokenHash, cancellationToken);

        if (dispositivo is null)
        {
            dispositivo = new DispositivoPush { TokenHash = tokenHash };
            context.DispositivosPush.Add(dispositivo);
        }

        dispositivo.UsuarioId = AuthClaims.ObtenerUsuarioId(User);
        dispositivo.Token = token;
        dispositivo.Plataforma = plataforma;
        dispositivo.ActualizadoEnUtc = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
