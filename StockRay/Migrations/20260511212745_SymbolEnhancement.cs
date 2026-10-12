using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockRay.Migrations
{
    /// <inheritdoc />
    public partial class SymbolEnhancement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Close",
                table: "Symbols",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "CurrentPrice",
                table: "Symbols",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "High",
                table: "Symbols",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<bool>(
                name: "IsTopNine",
                table: "Symbols",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "Low",
                table: "Symbols",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Open",
                table: "Symbols",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Close",
                table: "Symbols");

            migrationBuilder.DropColumn(
                name: "CurrentPrice",
                table: "Symbols");

            migrationBuilder.DropColumn(
                name: "High",
                table: "Symbols");

            migrationBuilder.DropColumn(
                name: "IsTopNine",
                table: "Symbols");

            migrationBuilder.DropColumn(
                name: "Low",
                table: "Symbols");

            migrationBuilder.DropColumn(
                name: "Open",
                table: "Symbols");
        }
    }
}
