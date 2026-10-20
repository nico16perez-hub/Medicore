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
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly MedicoreDbContext context;
    private readonly TokenService tokenService;

    public AuthController(MedicoreDbContext context, TokenService tokenService)
    {
        this.context = context;
        this.tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<SesionDto>> Login(LoginRequest request)
    {
        var email = request.Email.Trim();
        var usuario = await context.Usuarios
            .Include(item => item.Rol)
            .SingleOrDefaultAsync(item => item.Email == email);

        if (usuario is null || !usuario.Activo ||
            (usuario.Rol is not null && !usuario.Rol.Activo) ||
            !PasswordHashService.Verificar(request.Password, usuario.PasswordHash))
        {
            return Unauthorized(new
            {
                codigo = "credenciales_invalidas",
                mensaje = "El email o la contraseña no son correctos."
            });
        }

        var sesion = tokenService.CrearSesion(usuario);
        context.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = usuario.Id,
            TokenHash = TokenService.HashearRefreshToken(sesion.RefreshToken),
            CreadoEn = DateTime.UtcNow,
            ExpiraEn = sesion.RefreshTokenExpiraEn
        });
        await context.SaveChangesAsync();

        return Ok(MapearSesion(usuario, sesion));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<SesionDto>> Refresh(RefreshRequest request)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var ahora = DateTime.UtcNow;
        var tokenHash = TokenService.HashearRefreshToken(request.RefreshToken);
        var tokensRevocados = await context.RefreshTokens
            .Where(token => token.TokenHash == tokenHash &&
                            token.RevocadoEn == null &&
                            token.ExpiraEn > ahora)
            .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.RevocadoEn, ahora));

        if (tokensRevocados != 1)
        {
            await transaction.RollbackAsync();
            return Unauthorized(new
            {
                codigo = "refresh_invalido",
                mensaje = "La sesión expiró o el token de renovación ya fue utilizado."
            });
        }

        var usuarioId = await context.RefreshTokens
            .Where(token => token.TokenHash == tokenHash)
            .Select(token => token.UsuarioId)
            .SingleAsync();
        var usuario = await context.Usuarios
            .Include(item => item.Rol)
            .SingleOrDefaultAsync(item => item.Id == usuarioId);

        if (usuario is null || !usuario.Activo ||
            (usuario.Rol is not null && !usuario.Rol.Activo))
        {
            await transaction.CommitAsync();
            return Unauthorized(new
            {
                codigo = "usuario_inactivo",
                mensaje = "El usuario ya no está habilitado para iniciar sesión."
            });
        }

        var sesion = tokenService.CrearSesion(usuario);
        context.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = usuario.Id,
            TokenHash = TokenService.HashearRefreshToken(sesion.RefreshToken),
            CreadoEn = ahora,
            ExpiraEn = sesion.RefreshTokenExpiraEn
        });
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(MapearSesion(usuario, sesion));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request)
    {
        var tokenId = AuthClaims.ObtenerTokenId(User);
        if (string.IsNullOrWhiteSpace(tokenId))
        {
            return Unauthorized(new
            {
                codigo = "token_invalido",
                mensaje = "No se pudo identificar la sesión autenticada."
            });
        }

        var ahora = DateTime.UtcNow;
        var expiraEn = AuthClaims.ObtenerExpiracionToken(User);
        await using var transaction = await context.Database.BeginTransactionAsync();
        await context.RevokedAccessTokens
            .Where(token => token.ExpiraEn <= ahora)
            .ExecuteDeleteAsync();
        context.RevokedAccessTokens.Add(new RevokedAccessToken
        {
            TokenId = tokenId,
            ExpiraEn = expiraEn
        });

        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var usuarioId = AuthClaims.ObtenerUsuarioId(User);
            var tokenHash = TokenService.HashearRefreshToken(request.RefreshToken);
            await context.RefreshTokens
                .Where(token => token.UsuarioId == usuarioId &&
                                token.TokenHash == tokenHash &&
                                token.RevocadoEn == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.RevocadoEn, ahora));
        }

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return NoContent();
    }

    private static SesionDto MapearSesion(Usuario usuario, SesionEmitida sesion) =>
        new(
            sesion.AccessToken,
            sesion.RefreshToken,
            sesion.AccessTokenExpiraEn,
            MapearUsuario(usuario));

    internal static UsuarioSesionDto MapearUsuario(Usuario usuario) =>
        new(
            usuario.Id,
            usuario.Email,
            usuario.Activo,
            usuario.Rol is null
                ? null
                : new RolSesionDto(usuario.Rol.Id, usuario.Rol.Nombre, usuario.Rol.Codigo));
}
