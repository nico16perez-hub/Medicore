using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medicore.Api.Migrations
{
    /// <inheritdoc />
    public partial class DirectorioEspecialidadesDoctores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DoctorPreferidoUsuarioId",
                table: "turnos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "doctores_especialidades",
                columns: table => new
                {
                    DoctorUsuarioId = table.Column<int>(type: "int", nullable: false),
                    Especialidad = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctores_especialidades", x => new { x.DoctorUsuarioId, x.Especialidad });
                    table.ForeignKey(
                        name: "FK_doctores_especialidades_usuarios_DoctorUsuarioId",
                        column: x => x.DoctorUsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "turnos",
                keyColumn: "Id",
                keyValue: 1,
                column: "DoctorPreferidoUsuarioId",
                value: null);

            migrationBuilder.UpdateData(
                table: "turnos",
                keyColumn: "Id",
                keyValue: 2,
                column: "DoctorPreferidoUsuarioId",
                value: null);

            migrationBuilder.UpdateData(
                table: "turnos",
                keyColumn: "Id",
                keyValue: 3,
                column: "DoctorPreferidoUsuarioId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_turnos_DoctorPreferidoUsuarioId",
                table: "turnos",
                column: "DoctorPreferidoUsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_turnos_usuarios_DoctorPreferidoUsuarioId",
                table: "turnos",
                column: "DoctorPreferidoUsuarioId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_turnos_usuarios_DoctorPreferidoUsuarioId",
                table: "turnos");

            migrationBuilder.DropTable(
                name: "doctores_especialidades");

            migrationBuilder.DropIndex(
                name: "IX_turnos_DoctorPreferidoUsuarioId",
                table: "turnos");

            migrationBuilder.DropColumn(
                name: "DoctorPreferidoUsuarioId",
                table: "turnos");
        }
    }
}
