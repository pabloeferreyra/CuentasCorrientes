using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuentasCorrientes.Migrations
{
    /// <inheritdoc />
    public partial class debcred : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Debt",
                table: "Movements",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Debt",
                table: "Movements");
        }
    }
}
