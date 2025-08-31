using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuentasCorrientes.Migrations
{
    /// <inheritdoc />
    public partial class removeddebtonmovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Debt",
                table: "Movements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Debt",
                table: "Movements",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
