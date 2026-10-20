using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Medicore.Api.Services;

public static class AuthClaims
{
    public static int ObtenerUsuarioId(ClaimsPrincipal principal)
    {
        var valor = principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, out var usuarioId)
            ? usuarioId
            : throw new InvalidOperationException("El token autenticado no contiene un identificador válido.");
    }

    public static string? ObtenerTokenId(ClaimsPrincipal principal) =>
        principal.FindFirstValue(JwtRegisteredClaimNames.Jti)
        ?? principal.FindFirstValue(ClaimTypes.SerialNumber);

    public static DateTime ObtenerExpiracionToken(ClaimsPrincipal principal)
    {
        var valor = principal.FindFirstValue(JwtRegisteredClaimNames.Exp)
            ?? principal.FindFirstValue(ClaimTypes.Expiration);
        if (!long.TryParse(valor, out var segundosUnix))
        {
            throw new InvalidOperationException("El token autenticado no contiene una expiración válida.");
        }

        return DateTimeOffset.FromUnixTimeSeconds(segundosUnix).UtcDateTime;
    }
}
