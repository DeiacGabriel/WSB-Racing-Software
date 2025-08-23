using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WSB_Management.Migrations
{
    /// <inheritdoc />
    public partial class AddedMaxPersons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "maxPersons",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "maxPersons",
                table: "Events");
        }
    }
}
