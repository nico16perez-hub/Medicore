using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Medicore.Api.Migrations
{
    /// <inheritdoc />
    public partial class Unidad5Autenticacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DoctorUsuarioId",
                table: "turnos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PacienteUsuarioId",
                table: "turnos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoctorUsuarioId",
                table: "historia_clinica",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PacienteUsuarioId",
                table: "historia_clinica",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Codigo = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RolId = table.Column<int>(type: "int", nullable: true),
                    Email = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_RolId",
                        column: x => x.RolId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreadoEn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpiraEn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RevocadoEn = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "Activo", "Codigo", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "paciente", "Paciente" },
                    { 2, true, "doctor", "Doctor" },
                    { 3, true, "secretaria", "Secretaria" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_turnos_DoctorUsuarioId",
                table: "turnos",
                column: "DoctorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_turnos_PacienteUsuarioId",
                table: "turnos",
                column: "PacienteUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_historia_clinica_DoctorUsuarioId",
                table: "historia_clinica",
                column: "DoctorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_historia_clinica_PacienteUsuarioId",
                table: "historia_clinica",
                column: "PacienteUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_TokenHash",
                table: "refresh_tokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UsuarioId_RevocadoEn_ExpiraEn",
                table: "refresh_tokens",
                columns: new[] { "UsuarioId", "RevocadoEn", "ExpiraEn" });

            migrationBuilder.CreateIndex(
                name: "IX_roles_Codigo",
                table: "roles",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_RolId",
                table: "usuarios",
                column: "RolId");

            migrationBuilder.AddForeignKey(
                name: "FK_historia_clinica_usuarios_DoctorUsuarioId",
                table: "historia_clinica",
                column: "DoctorUsuarioId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_historia_clinica_usuarios_PacienteUsuarioId",
                table: "historia_clinica",
                column: "PacienteUsuarioId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_turnos_usuarios_DoctorUsuarioId",
                table: "turnos",
                column: "DoctorUsuarioId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_turnos_usuarios_PacienteUsuarioId",
                table: "turnos",
                column: "PacienteUsuarioId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_historia_clinica_usuarios_DoctorUsuarioId",
                table: "historia_clinica");

            migrationBuilder.DropForeignKey(
                name: "FK_historia_clinica_usuarios_PacienteUsuarioId",
                table: "historia_clinica");

            migrationBuilder.DropForeignKey(
                name: "FK_turnos_usuarios_DoctorUsuarioId",
                table: "turnos");

            migrationBuilder.DropForeignKey(
                name: "FK_turnos_usuarios_PacienteUsuarioId",
                table: "turnos");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropIndex(
                name: "IX_turnos_DoctorUsuarioId",
                table: "turnos");

            migrationBuilder.DropIndex(
                name: "IX_turnos_PacienteUsuarioId",
                table: "turnos");

            migrationBuilder.DropIndex(
                name: "IX_historia_clinica_DoctorUsuarioId",
                table: "historia_clinica");

            migrationBuilder.DropIndex(
                name: "IX_historia_clinica_PacienteUsuarioId",
                table: "historia_clinica");

            migrationBuilder.DropColumn(
                name: "DoctorUsuarioId",
                table: "turnos");

            migrationBuilder.DropColumn(
                name: "PacienteUsuarioId",
                table: "turnos");

            migrationBuilder.DropColumn(
                name: "DoctorUsuarioId",
                table: "historia_clinica");

            migrationBuilder.DropColumn(
                name: "PacienteUsuarioId",
                table: "historia_clinica");
        }
    }
}
