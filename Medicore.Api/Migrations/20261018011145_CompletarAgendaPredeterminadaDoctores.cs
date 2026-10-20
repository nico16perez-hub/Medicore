using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medicore.Api.Migrations
{
    /// <inheritdoc />
    public partial class CompletarAgendaPredeterminadaDoctores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
