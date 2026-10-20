using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Medicore.Api.Migrations
{
    /// <inheritdoc />
    public partial class LimpiarSeedUsuariosDeDesarrollo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "historia_clinica",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DoctorUsuarioId", "PacienteUsuarioId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "historia_clinica",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DoctorUsuarioId", "PacienteUsuarioId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "historia_clinica",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DoctorUsuarioId", "PacienteUsuarioId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "historia_clinica",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DoctorUsuarioId", "PacienteUsuarioId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "turnos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DoctorUsuarioId", "PacienteUsuarioId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "turnos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DoctorUsuarioId", "PacienteUsuarioId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "turnos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DoctorUsuarioId", "PacienteUsuarioId" },
                values: new object[] { null, null });

            migrationBuilder.DeleteData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
