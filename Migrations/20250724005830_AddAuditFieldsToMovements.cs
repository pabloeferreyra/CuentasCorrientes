using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuentasCorrientes.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsToMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Movements",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Movements",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "Movements",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByUserId",
                table: "Movements",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Movements");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Movements");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "Movements");

            migrationBuilder.DropColumn(
                name: "ModifiedByUserId",
                table: "Movements");
        }
    }
}
