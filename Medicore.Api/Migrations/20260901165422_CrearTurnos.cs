using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Medicore.Api.Migrations
{
    /// <inheritdoc />
    public partial class CrearTurnos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "turnos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Profesional = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Especialidad = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Hora = table.Column<TimeOnly>(type: "time", nullable: false),
                    Estado = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_turnos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "turnos",
                columns: new[] { "Id", "Especialidad", "Estado", "Fecha", "Hora", "Profesional" },
                values: new object[,]
                {
                    { 1, "Diabetologia", "Confirmado", new DateOnly(2026, 8, 25), new TimeOnly(10, 0, 0), "Dr. Martinez" },
                    { 2, "Nutricion", "Pendiente", new DateOnly(2026, 8, 28), new TimeOnly(15, 30, 0), "Lic. Gomez" },
                    { 3, "Cardiologia", "Confirmado", new DateOnly(2026, 9, 2), new TimeOnly(9, 15, 0), "Dra. Fernandez" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "turnos");
        }
    }
}
