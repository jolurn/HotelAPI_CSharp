using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoBloqueado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EstaBloqueado",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstaBloqueado",
                table: "Usuarios");
        }
    }
}
