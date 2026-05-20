using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotTracked.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIssueWithGanttAndBurnDownFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstimatedSeconds",
                table: "Issues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Issues",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedSeconds",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Issues");
        }
    }
}
