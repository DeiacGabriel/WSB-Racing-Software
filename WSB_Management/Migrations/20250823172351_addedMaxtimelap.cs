using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WSB_Management.Migrations
{
    /// <inheritdoc />
    public partial class addedMaxtimelap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "MaxTimelap",
                table: "Gruppes",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "BestTime",
                table: "Customers",
                type: "time(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxTimelap",
                table: "Gruppes");

            migrationBuilder.DropColumn(
                name: "BestTime",
                table: "Customers");
        }
    }
}
