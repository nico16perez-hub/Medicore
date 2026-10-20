using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Medicore.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace Medicore.Api.Services;

public class TokenService
{
    private readonly string issuer;
    private readonly string audience;
    private readonly SymmetricSecurityKey signingKey;
    private readonly TimeSpan accessTokenDuration;
    private readonly TimeSpan refreshTokenDuration;

    public TokenService(IConfiguration configuration)
    {
        issuer = configuration["Authentication:Jwt:Issuer"] ?? "Medicore";
        audience = configuration["Authentication:Jwt:Audience"] ?? "Medicore.Client";
        var key = configuration["Authentication:Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException(
                "Configure Authentication:Jwt:SigningKey con al menos 32 bytes antes de iniciar la API.");
        }

        signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        accessTokenDuration = TimeSpan.FromMinutes(
            configuration.GetValue("Authentication:Jwt:AccessTokenMinutes", 15));
        refreshTokenDuration = TimeSpan.FromDays(
            configuration.GetValue("Authentication:Jwt:RefreshTokenDays", 7));
        if (accessTokenDuration <= TimeSpan.Zero || refreshTokenDuration <= TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                "Las duraciones de access token y refresh token deben ser mayores que cero.");
        }
    }

    public SesionEmitida CrearSesion(Usuario usuario)
    {
        var ahora = DateTime.UtcNow;
        var expira = ahora.Add(accessTokenDuration);
        var claims = new List<Claim>
        {
            new("sub", usuario.Id.ToString()),
            new("email", usuario.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (usuario.Rol is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, usuario.Rol.Codigo));
        }

        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer, audience, claims, ahora, expira, credentials);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        return new SesionEmitida(
            new JwtSecurityTokenHandler().WriteToken(jwt),
            expira,
            refreshToken,
            ahora.Add(refreshTokenDuration));
    }

    public static string HashearRefreshToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}

public record SesionEmitida(
    string AccessToken,
    DateTime AccessTokenExpiraEn,
    string RefreshToken,
    DateTime RefreshTokenExpiraEn);
