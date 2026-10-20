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
[Route("api/perfil")]
[Authorize]
public class PerfilController : ControllerBase
{
    private const long TamanoMaximoFoto = 5 * 1024 * 1024;
    private const long TamanoMaximoSolicitud = TamanoMaximoFoto + 64 * 1024;
    private readonly MedicoreDbContext context;
    private readonly IWebHostEnvironment environment;
    private readonly ILogger<PerfilController> logger;

    public PerfilController(
        MedicoreDbContext context,
        IWebHostEnvironment environment,
        ILogger<PerfilController> logger)
    {
        this.context = context;
        this.environment = environment;
        this.logger = logger;
    }

    [HttpGet("foto")]
    public async Task<IActionResult> ObtenerFoto(CancellationToken cancellationToken)
    {
        Response.Headers["Cache-Control"] = "private, no-store";
        var usuarioId = AuthClaims.ObtenerUsuarioId(User);
        var nombre = await context.Usuarios
            .Where(usuario => usuario.Id == usuarioId)
            .Select(usuario => usuario.FotoPerfilNombre)
            .SingleOrDefaultAsync(cancellationToken);

        if (nombre is null)
        {
            return NoContent();
        }

        var ruta = RutaFoto(nombre);
        if (!System.IO.File.Exists(ruta))
        {
            return NotFound(new { codigo = "foto_no_encontrada", mensaje = "No se encontró la foto del perfil." });
        }

        return PhysicalFile(ruta, TipoContenido(nombre));
    }

    [HttpPost("foto")]
    [RequestSizeLimit(TamanoMaximoSolicitud)]
    public async Task<IActionResult> GuardarFoto(
        [FromForm] IFormFile? foto,
        CancellationToken cancellationToken)
    {
        if (foto is null || foto.Length == 0)
        {
            return BadRequest(new { codigo = "foto_requerida", mensaje = "Seleccioná una imagen para guardar." });
        }

        if (foto.Length > TamanoMaximoFoto)
        {
            return BadRequest(new { codigo = "foto_tamano_invalido", mensaje = "La imagen no puede superar los 5 MB." });
        }

        var formato = await DetectarFormatoAsync(foto, cancellationToken);
        if (formato is null)
        {
            return BadRequest(new { codigo = "foto_formato_invalido", mensaje = "Usá una imagen JPEG, PNG o WebP válida." });
        }

        var usuarioId = AuthClaims.ObtenerUsuarioId(User);
        var usuario = await context.Usuarios
            .SingleOrDefaultAsync(item => item.Id == usuarioId, cancellationToken);
        if (usuario is null)
        {
            return NotFound(new { codigo = "usuario_no_encontrado", mensaje = "No se encontró el perfil autenticado." });
        }

        var nombreAnterior = usuario.FotoPerfilNombre;
        var nombreNuevo = $"{Guid.NewGuid():N}.{formato.Value.Extension}";
        var ruta = RutaFoto(nombreNuevo);
        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);

        try
        {
            await using (var salida = new FileStream(
                ruta, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await foto.CopyToAsync(salida, cancellationToken);
            }

            usuario.FotoPerfilNombre = nombreNuevo;
            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (System.IO.File.Exists(ruta))
            {
                System.IO.File.Delete(ruta);
            }
            throw;
        }

        if (nombreAnterior is not null)
        {
            try
            {
                System.IO.File.Delete(RutaFoto(nombreAnterior));
            }
            catch (IOException error)
            {
                logger.LogWarning(error, "No se pudo limpiar la foto de perfil anterior del usuario {UsuarioId}.", usuarioId);
            }
            catch (UnauthorizedAccessException error)
            {
                logger.LogWarning(error, "No se pudo limpiar la foto de perfil anterior del usuario {UsuarioId}.", usuarioId);
            }
        }

        return NoContent();
    }

    [HttpGet("ubicacion")]
    [Authorize(Roles = "paciente")]
    public async Task<ActionResult<UbicacionPerfilDto>> ObtenerUbicacion(
        CancellationToken cancellationToken)
    {
        var usuarioId = AuthClaims.ObtenerUsuarioId(User);
        var ubicacion = await context.Usuarios
            .AsNoTracking()
            .Where(usuario => usuario.Id == usuarioId &&
                usuario.UbicacionLatitud.HasValue &&
                usuario.UbicacionLongitud.HasValue &&
                usuario.UbicacionActualizadaEnUtc.HasValue)
            .Select(usuario => new UbicacionPerfilDto(
                usuario.UbicacionLatitud!.Value,
                usuario.UbicacionLongitud!.Value,
                usuario.UbicacionActualizadaEnUtc!.Value))
            .SingleOrDefaultAsync(cancellationToken);

        if (ubicacion is null)
        {
            return NoContent();
        }

        return Ok(ubicacion);
    }

    [HttpPut("ubicacion")]
    [Authorize(Roles = "paciente")]
    public async Task<IActionResult> GuardarUbicacion(
        UbicacionRequest request,
        CancellationToken cancellationToken)
    {
        var usuarioId = AuthClaims.ObtenerUsuarioId(User);
        var usuario = await context.Usuarios
            .SingleOrDefaultAsync(item => item.Id == usuarioId, cancellationToken);
        if (usuario is null)
        {
            return NotFound(new { codigo = "usuario_no_encontrado", mensaje = "No se encontró el perfil autenticado." });
        }

        usuario.UbicacionLatitud = request.Latitud;
        usuario.UbicacionLongitud = request.Longitud;
        usuario.UbicacionActualizadaEnUtc = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("ubicacion")]
    [Authorize(Roles = "paciente")]
    public async Task<IActionResult> EliminarUbicacion(CancellationToken cancellationToken)
    {
        var usuarioId = AuthClaims.ObtenerUsuarioId(User);
        var usuario = await context.Usuarios
            .SingleOrDefaultAsync(item => item.Id == usuarioId, cancellationToken);
        if (usuario is null)
        {
            return NotFound(new { codigo = "usuario_no_encontrado", mensaje = "No se encontró el perfil autenticado." });
        }

        usuario.UbicacionLatitud = null;
        usuario.UbicacionLongitud = null;
        usuario.UbicacionActualizadaEnUtc = null;
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private string RutaFoto(string nombre)
    {
        if (Path.GetFileName(nombre) != nombre)
        {
            throw new InvalidOperationException("El nombre de la foto de perfil almacenado no es válido.");
        }

        return Path.Combine(environment.ContentRootPath, "App_Data", "profile-images", nombre);
    }

    private static string TipoContenido(string nombre) =>
        Path.GetExtension(nombre).ToLowerInvariant() switch
        {
            ".jpg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => throw new InvalidOperationException("El tipo de foto de perfil almacenado no es válido.")
        };

    private static async Task<(string Extension, string ContentType)?> DetectarFormatoAsync(
        IFormFile foto,
        CancellationToken cancellationToken)
    {
        var firma = new byte[12];
        await using var flujo = foto.OpenReadStream();
        var leidos = await flujo.ReadAsync(firma.AsMemory(), cancellationToken);

        if (leidos >= 3 && firma[0] == 0xFF && firma[1] == 0xD8 && firma[2] == 0xFF &&
            foto.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
        {
            return ("jpg", "image/jpeg");
        }

        if (leidos >= 8 &&
            firma.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }) &&
            foto.ContentType.Equals("image/png", StringComparison.OrdinalIgnoreCase))
        {
            return ("png", "image/png");
        }

        if (leidos >= 12 &&
            firma.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
            firma.AsSpan(8, 4).SequenceEqual("WEBP"u8) &&
            foto.ContentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase))
        {
            return ("webp", "image/webp");
        }

        return null;
    }
}
