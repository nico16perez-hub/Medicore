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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Turno>(entity =>
        {
            entity.ToTable("turnos");
            entity.HasKey(turno => turno.Id);

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
    }
}
