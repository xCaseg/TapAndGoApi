using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TapAndGo.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Elimina la columna antigua de tipo int
            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "MenuItems");

            // Crea la nueva columna de tipo string (nullable o no)
            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "MenuItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: ""); // o nullable: true
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
