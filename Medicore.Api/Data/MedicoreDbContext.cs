using Medicore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Medicore.Api.Data;

public class MedicoreDbContext : DbContext
{
    public MedicoreDbContext(DbContextOptions<MedicoreDbContext> options) : base(options)
    {
    }

    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<ItemNutricion> PlanNutricional => Set<ItemNutricion>();
    public DbSet<RegistroHistoriaClinica> HistoriaClinica => Set<RegistroHistoriaClinica>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<RevokedAccessToken> RevokedAccessTokens => Set<RevokedAccessToken>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<DispositivoPush> DispositivosPush => Set<DispositivoPush>();
    public DbSet<DisponibilidadDoctor> DisponibilidadesDoctores => Set<DisponibilidadDoctor>();
    public DbSet<DoctorEspecialidad> DoctoresEspecialidades => Set<DoctorEspecialidad>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Turno>(entity =>
        {
            entity.ToTable("turnos");
            entity.HasKey(turno => turno.Id);
            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(turno => turno.PacienteUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(turno => turno.DoctorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(turno => turno.DoctorPreferidoUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(turno => turno.Profesional)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(turno => turno.Especialidad)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(turno => turno.Estado)
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(turno => turno.Fecha)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(turno => turno.Hora)
                .HasColumnType("time")
                .IsRequired();

            entity.HasData(
                new Turno
                {
                    Id = 1,
                    Profesional = "Dr. Martinez",
                    Especialidad = "Diabetologia",
                    Fecha = new DateOnly(2026, 8, 25),
                    Hora = new TimeOnly(10, 0),
                    Estado = "Confirmado"
                },
                new Turno
                {
                    Id = 2,
                    Profesional = "Lic. Gomez",
                    Especialidad = "Nutricion",
                    Fecha = new DateOnly(2026, 8, 28),
                    Hora = new TimeOnly(15, 30),
                    Estado = "Pendiente"
                },
                new Turno
                {
                    Id = 3,
                    Profesional = "Dra. Fernandez",
                    Especialidad = "Cardiologia",
                    Fecha = new DateOnly(2026, 9, 2),
                    Hora = new TimeOnly(9, 15),
                    Estado = "Confirmado"
                }
            );
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.Property(usuario => usuario.FotoPerfilNombre)
                .HasMaxLength(40);
            entity.Property(usuario => usuario.UbicacionLatitud)
                .HasColumnType("decimal(10,7)");
            entity.Property(usuario => usuario.UbicacionLongitud)
                .HasColumnType("decimal(10,7)");
            entity.Property(usuario => usuario.UbicacionActualizadaEnUtc);
        });

        modelBuilder.Entity<DisponibilidadDoctor>(entity =>
        {
            entity.ToTable("disponibilidades_doctores");
            entity.HasKey(disponibilidad => disponibilidad.Id);
            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(disponibilidad => disponibilidad.DoctorUsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(disponibilidad => new
            {
                disponibilidad.DoctorUsuarioId,
                disponibilidad.DiaSemana,
                disponibilidad.HoraInicio
            }).IsUnique();
            entity.Property(disponibilidad => disponibilidad.DiaSemana)
                .HasConversion<int>()
                .IsRequired();
            entity.Property(disponibilidad => disponibilidad.HoraInicio)
                .HasColumnType("time")
                .IsRequired();
            entity.Property(disponibilidad => disponibilidad.HoraFin)
                .HasColumnType("time")
                .IsRequired();
        });

        modelBuilder.Entity<DoctorEspecialidad>(entity =>
        {
            entity.ToTable("doctores_especialidades");
            entity.HasKey(item => new { item.DoctorUsuarioId, item.Especialidad });
            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(item => item.DoctorUsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(item => item.Especialidad)
                .HasMaxLength(120)
                .IsRequired();
        });

        modelBuilder.Entity<ItemNutricion>(entity =>
        {
            entity.ToTable("plan_nutricional");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.Dia)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(item => item.Comida)
                .HasMaxLength(60)
                .IsRequired();

            entity.Property(item => item.Detalle)
                .HasMaxLength(240)
                .IsRequired();

            entity.HasData(
                new ItemNutricion
                {
                    Id = 1,
                    Dia = "Lunes",
                    Comida = "Desayuno",
                    Detalle = "Avena con frutas y te verde"
                },
                new ItemNutricion
                {
                    Id = 2,
                    Dia = "Lunes",
                    Comida = "Almuerzo",
                    Detalle = "Pechuga de pollo a la plancha con ensalada"
                },
                new ItemNutricion
                {
                    Id = 3,
                    Dia = "Lunes",
                    Comida = "Cena",
                    Detalle = "Sopa de verduras y pescado"
                },
                new ItemNutricion
                {
                    Id = 4,
                    Dia = "Martes",
                    Comida = "Desayuno",
                    Detalle = "Yogur descremado con granola"
                }
            );
        });

        modelBuilder.Entity<RegistroHistoriaClinica>(entity =>
        {
            entity.ToTable("historia_clinica");
            entity.HasKey(registro => registro.Id);
            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(registro => registro.PacienteUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(registro => registro.DoctorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(registro => registro.Fecha)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(registro => registro.Tipo)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(registro => registro.Valor)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(registro => registro.Notas)
                .HasMaxLength(240)
                .IsRequired();

            entity.HasData(
                new RegistroHistoriaClinica
                {
                    Id = 1,
                    Fecha = new DateOnly(2026, 8, 15),
                    Tipo = "Glucemia",
                    Valor = "110 mg/dL",
                    Notas = "En ayunas"
                },
                new RegistroHistoriaClinica
                {
                    Id = 2,
                    Fecha = new DateOnly(2026, 8, 16),
                    Tipo = "Presion Arterial",
                    Valor = "120/80 mmHg",
                    Notas = "Normal"
                },
                new RegistroHistoriaClinica
                {
                    Id = 3,
                    Fecha = new DateOnly(2026, 8, 18),
                    Tipo = "Peso",
                    Valor = "78 kg",
                    Notas = "Control mensual"
                },
                new RegistroHistoriaClinica
                {
                    Id = 4,
                    Fecha = new DateOnly(2026, 8, 20),
                    Tipo = "Glucemia",
                    Valor = "125 mg/dL",
                    Notas = "Post-prandial"
                }
            );
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(rol => rol.Id);
            entity.Property(rol => rol.Nombre).HasMaxLength(80).IsRequired();
            entity.Property(rol => rol.Codigo).HasMaxLength(40).IsRequired();
            entity.HasIndex(rol => rol.Codigo).IsUnique();
            entity.HasData(
                new Rol { Id = 1, Nombre = "Paciente", Codigo = "paciente", Activo = true },
                new Rol { Id = 2, Nombre = "Doctor", Codigo = "doctor", Activo = true },
                new Rol { Id = 3, Nombre = "Secretaria", Codigo = "secretaria", Activo = true });
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(usuario => usuario.Id);
            entity.Property(usuario => usuario.Email).HasMaxLength(254).IsRequired();
            entity.Property(usuario => usuario.PasswordHash).HasMaxLength(200).IsRequired();
            entity.HasIndex(usuario => usuario.Email).IsUnique();
            entity.HasOne(usuario => usuario.Rol)
                .WithMany(rol => rol.Usuarios)
                .HasForeignKey(usuario => usuario.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(token => token.Id);
            entity.Property(token => token.TokenHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.HasIndex(token => new { token.UsuarioId, token.RevocadoEn, token.ExpiraEn });
            entity.HasOne(token => token.Usuario)
                .WithMany(usuario => usuario.RefreshTokens)
                .HasForeignKey(token => token.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RevokedAccessToken>(entity =>
        {
            entity.ToTable("revoked_access_tokens");
            entity.HasKey(token => token.Id);
            entity.Property(token => token.TokenId).HasMaxLength(64).IsRequired();
            entity.HasIndex(token => new { token.TokenId, token.ExpiraEn });
            entity.HasIndex(token => token.ExpiraEn);
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.ToTable("notificaciones");
            entity.HasKey(notificacion => notificacion.Id);
            entity.Property(notificacion => notificacion.Titulo).HasMaxLength(120).IsRequired();
            entity.Property(notificacion => notificacion.Mensaje).HasMaxLength(500).IsRequired();
            entity.Property(notificacion => notificacion.CreadaEnUtc).IsRequired();
            entity.HasIndex(notificacion => new
            {
                notificacion.UsuarioId,
                notificacion.Leida,
                notificacion.CreadaEnUtc
            });
            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(notificacion => notificacion.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(notificacion => notificacion.Turno)
                .WithMany()
                .HasForeignKey(notificacion => notificacion.TurnoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DispositivoPush>(entity =>
        {
            entity.ToTable("dispositivos_push");
            entity.HasKey(dispositivo => dispositivo.Id);
            entity.Property(dispositivo => dispositivo.Token).HasColumnType("text").IsRequired();
            entity.Property(dispositivo => dispositivo.TokenHash).HasMaxLength(64).IsRequired();
            entity.Property(dispositivo => dispositivo.Plataforma).HasMaxLength(20).IsRequired();
            entity.HasIndex(dispositivo => dispositivo.TokenHash).IsUnique();
            entity.HasIndex(dispositivo => dispositivo.UsuarioId);
            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(dispositivo => dispositivo.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
