using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Medicore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarModeloCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "historia_clinica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Tipo = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Valor = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notas = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historia_clinica", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "plan_nutricional",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Dia = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Comida = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Detalle = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_nutricional", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "historia_clinica",
                columns: new[] { "Id", "Fecha", "Notas", "Tipo", "Valor" },
                values: new object[,]
                {
                    { 1, new DateOnly(2026, 8, 15), "En ayunas", "Glucemia", "110 mg/dL" },
                    { 2, new DateOnly(2026, 8, 16), "Normal", "Presion Arterial", "120/80 mmHg" },
                    { 3, new DateOnly(2026, 8, 18), "Control mensual", "Peso", "78 kg" },
                    { 4, new DateOnly(2026, 8, 20), "Post-prandial", "Glucemia", "125 mg/dL" }
                });

            migrationBuilder.InsertData(
                table: "plan_nutricional",
                columns: new[] { "Id", "Comida", "Detalle", "Dia" },
                values: new object[,]
                {
                    { 1, "Desayuno", "Avena con frutas y te verde", "Lunes" },
                    { 2, "Almuerzo", "Pechuga de pollo a la plancha con ensalada", "Lunes" },
                    { 3, "Cena", "Sopa de verduras y pescado", "Lunes" },
                    { 4, "Desayuno", "Yogur descremado con granola", "Martes" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historia_clinica");

            migrationBuilder.DropTable(
                name: "plan_nutricional");
        }
    }
}
