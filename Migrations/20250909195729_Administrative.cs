using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuentasCorrientes.Migrations
{
    /// <inheritdoc />
    public partial class Administrative : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Administrative",
                table: "Clients",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Administrative",
                table: "Clients");
        }
    }
}
