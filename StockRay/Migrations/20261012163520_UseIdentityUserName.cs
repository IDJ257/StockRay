using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockRay.Migrations
{
    /// <inheritdoc />
    public partial class UseIdentityUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE [Users] SET [Name] = [UserName];");
        }
    }
}
