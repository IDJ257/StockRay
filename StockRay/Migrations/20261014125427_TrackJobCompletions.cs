using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockRay.Migrations
{
    /// <inheritdoc />
    public partial class TrackJobCompletions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobCompletions",
                columns: table => new
                {
                    JobName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastCompletedScheduledTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobCompletions", x => x.JobName);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobCompletions");
        }
    }
}
