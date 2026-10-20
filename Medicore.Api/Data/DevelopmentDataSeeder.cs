using Medicore.Api.Models;
using Medicore.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Medicore.Api.Data;

public class DevelopmentDataSeeder : IHostedService
{
    private const string PasswordDesarrollo = "MediCoreDev!2026";
    private readonly IServiceScopeFactory scopeFactory;

    public DevelopmentDataSeeder(IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicoreDbContext>();
        var roles = await context.Roles
            .AsNoTracking()
            .Where(rol => rol.Activo)
            .ToDictionaryAsync(rol => rol.Codigo, rol => rol.Id, cancellationToken);

        var codigosRequeridos = new[] { "paciente", "doctor", "secretaria" };
        if (codigosRequeridos.Any(codigo => !roles.ContainsKey(codigo)))
        {
            throw new InvalidOperationException(
                "No se encontraron los roles iniciales. Ejecuta `dotnet ef database update` antes de iniciar MediCore.");
        }

        var cuentas = new (string Email, string? Rol)[]
        {
            ("paciente@medicore.local", "paciente"),
            ("doctor@medicore.local", "doctor"),
            ("secretaria@medicore.local", "secretaria"),
            ("sinrol@medicore.local", null)
        };

        foreach (var cuenta in cuentas)
        {
            if (await context.Usuarios.AnyAsync(usuario => usuario.Email == cuenta.Email, cancellationToken))
            {
                continue;
            }

            context.Usuarios.Add(new Usuario
            {
                RolId = cuenta.Rol is null ? null : roles[cuenta.Rol],
                Email = cuenta.Email,
                PasswordHash = PasswordHashService.Hash(PasswordDesarrollo),
                Activo = true
            });
        }
        await context.SaveChangesAsync(cancellationToken);

        var pacienteId = await context.Usuarios
            .Where(usuario => usuario.Email == "paciente@medicore.local")
            .Select(usuario => usuario.Id)
            .SingleAsync(cancellationToken);
        var doctorId = await context.Usuarios
            .Where(usuario => usuario.Email == "doctor@medicore.local")
            .Select(usuario => usuario.Id)
            .SingleAsync(cancellationToken);

        await context.Turnos
            .Where(turno => turno.Id <= 3 &&
                            turno.PacienteUsuarioId == null &&
                            turno.DoctorUsuarioId == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(turno => turno.PacienteUsuarioId, pacienteId)
                .SetProperty(turno => turno.DoctorUsuarioId, doctorId), cancellationToken);
        await context.HistoriaClinica
            .Where(registro => registro.Id <= 4 &&
                               registro.PacienteUsuarioId == null &&
                               registro.DoctorUsuarioId == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(registro => registro.PacienteUsuarioId, pacienteId)
                .SetProperty(registro => registro.DoctorUsuarioId, doctorId), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
