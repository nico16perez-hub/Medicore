using System.Text.Json;
using Medicore.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string PoliticaCorsMedicore = "MedicoreCors";

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddDbContext<MedicoreDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Medicore"),
        new MySqlServerVersion(new Version(8, 0, 36))
    ));

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
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    });

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(PoliticaCorsMedicore);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();
