using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medicore.Api.Migrations
{
    /// <inheritdoc />
    public partial class Unidad7UbicacionNotificacionesPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UbicacionActualizadaEnUtc",
                table: "usuarios",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UbicacionLatitud",
                table: "usuarios",
                type: "decimal(10,7)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UbicacionLongitud",
                table: "usuarios",
                type: "decimal(10,7)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "dispositivos_push",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TokenHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Plataforma = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActualizadoEnUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispositivos_push", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dispositivos_push_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Mensaje = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreadaEnUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Leida = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TurnoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notificaciones_turnos_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "turnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_notificaciones_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_dispositivos_push_TokenHash",
                table: "dispositivos_push",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dispositivos_push_UsuarioId",
                table: "dispositivos_push",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_notificaciones_TurnoId",
                table: "notificaciones",
                column: "TurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_notificaciones_UsuarioId_Leida_CreadaEnUtc",
                table: "notificaciones",
                columns: new[] { "UsuarioId", "Leida", "CreadaEnUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dispositivos_push");

            migrationBuilder.DropTable(
                name: "notificaciones");

            migrationBuilder.DropColumn(
                name: "UbicacionActualizadaEnUtc",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "UbicacionLatitud",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "UbicacionLongitud",
                table: "usuarios");
        }
    }
}
