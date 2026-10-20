using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medicore.Api.Migrations
{
    /// <inheritdoc />
    public partial class Unidad6FotoPerfil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoPerfilNombre",
                table: "usuarios",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoPerfilNombre",
                table: "usuarios");
        }
    }
}
