using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ItAssets.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Assets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Assets");
        }
    }
}
