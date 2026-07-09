using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ItAssets.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierAndDateReceived : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateReceived",
                table: "Assets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Supplier",
                table: "Assets",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateReceived",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Supplier",
                table: "Assets");
        }
    }
}
