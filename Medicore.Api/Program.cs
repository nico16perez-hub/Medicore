using System.Text.Json;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Medicore.Api.Data;
using Medicore.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

const string PoliticaCorsMedicore = "MedicoreCors";

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddDbContext<MedicoreDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Medicore"),
        new MySqlServerVersion(new Version(8, 0, 36))
    ));

var signingKey = builder.Configuration["Authentication:Jwt:SigningKey"];
if (string.IsNullOrWhiteSpace(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32)
{
    throw new InvalidOperationException(
        "Configure Authentication:Jwt:SigningKey con al menos 32 bytes antes de iniciar la API.");
}

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
var issuer = builder.Configuration["Authentication:Jwt:Issuer"] ?? "Medicore";
var audience = builder.Configuration["Authentication:Jwt:Audience"] ?? "Medicore.Client";

builder.Services.AddSingleton<TokenService>();
builder.Services.AddScoped<NotificacionesTurnosService>();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<DevelopmentDataSeeder>();
}
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(
                builder.Configuration.GetValue("Authentication:Jwt:ClockSkewSeconds", 30)),
            NameClaimType = "email",
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var sub = context.Principal?.FindFirst("sub")?.Value
                    ?? context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(sub, out var usuarioId))
                {
                    context.Fail("El token no contiene un identificador de usuario válido.");
                    return;
                }

                var database = context.HttpContext.RequestServices
                    .GetRequiredService<MedicoreDbContext>();
                var tokenId = (context.SecurityToken as JwtSecurityToken)?.Id
                    ?? AuthClaims.ObtenerTokenId(context.Principal!);
                if (string.IsNullOrWhiteSpace(tokenId))
                {
                    context.Fail("El token no contiene un identificador de sesión válido.");
                    return;
                }

                var ahora = DateTime.UtcNow;
                var tokenRevocado = await database.RevokedAccessTokens
                    .AsNoTracking()
                    .AnyAsync(item => item.TokenId == tokenId && item.ExpiraEn > ahora,
                        context.HttpContext.RequestAborted);
                if (tokenRevocado)
                {
                    context.Fail("La sesión fue cerrada.");
                    return;
                }

                var usuario = await database.Usuarios
                    .AsNoTracking()
                    .Include(item => item.Rol)
                    .SingleOrDefaultAsync(item => item.Id == usuarioId, context.HttpContext.RequestAborted);

                if (usuario is null || !usuario.Activo ||
                    (usuario.Rol is not null && !usuario.Rol.Activo))
                {
                    context.Fail("El usuario ya no está habilitado.");
                    return;
                }

                var rolToken = context.Principal?.FindFirst(ClaimTypes.Role)?.Value
                    ?? context.Principal?.FindFirst("role")?.Value;
                if (!string.Equals(rolToken, usuario.Rol?.Codigo, StringComparison.Ordinal))
                {
                    context.Fail("El rol del token ya no coincide con el rol vigente.");
                }
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(PoliticaCorsMedicore, policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var mensaje = string.Join(" ", context.ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "El valor enviado no es válido."
                    : error.ErrorMessage));

            return new BadRequestObjectResult(new
            {
                codigo = "datos_invalidos",
                mensaje
            });
        };
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    });

builder.Services.AddOpenApi();

var app = builder.Build();
_ = app.Services.GetRequiredService<TokenService>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(PoliticaCorsMedicore);
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapControllers();

app.Run();
