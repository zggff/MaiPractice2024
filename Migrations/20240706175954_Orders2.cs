using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaiPractice.Migrations
{
    /// <inheritdoc />
    public partial class Orders2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Placed",
                table: "Orders",
                newName: "PlacedDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "PlacedDate",
                table: "Orders",
                newName: "Placed");
        }
    }
}
