using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medicore.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgendaSemanalDoctores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "disponibilidades_doctores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DoctorUsuarioId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_disponibilidades_doctores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_disponibilidades_doctores_usuarios_DoctorUsuarioId",
                        column: x => x.DoctorUsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_disponibilidades_doctores_DoctorUsuarioId_DiaSemana_HoraInic~",
                table: "disponibilidades_doctores",
                columns: new[] { "DoctorUsuarioId", "DiaSemana", "HoraInicio" },
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO `disponibilidades_doctores`
                    (`DoctorUsuarioId`, `DiaSemana`, `HoraInicio`, `HoraFin`)
                SELECT doctor.`Id`, dias.`DiaSemana`, '08:00:00', '13:00:00'
                FROM `usuarios` AS doctor
                INNER JOIN `roles` AS rol ON rol.`Id` = doctor.`RolId`
                CROSS JOIN (
                    SELECT 1 AS `DiaSemana`
                    UNION ALL SELECT 2
                    UNION ALL SELECT 3
                    UNION ALL SELECT 4
                    UNION ALL SELECT 5
                ) AS dias
                WHERE doctor.`Activo` = 1
                  AND rol.`Activo` = 1
                  AND rol.`Codigo` = 'doctor'
                  AND NOT EXISTS (
                      SELECT 1
                      FROM `disponibilidades_doctores` AS existente
                      WHERE existente.`DoctorUsuarioId` = doctor.`Id`
                        AND existente.`DiaSemana` = dias.`DiaSemana`
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "disponibilidades_doctores");
        }
    }
}
