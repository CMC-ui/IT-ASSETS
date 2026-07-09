using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ItAssets.Migrations
{
    /// <inheritdoc />
    public partial class AddUserExtension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Extension",
                table: "AspNetUsers");
        }
    }
}
