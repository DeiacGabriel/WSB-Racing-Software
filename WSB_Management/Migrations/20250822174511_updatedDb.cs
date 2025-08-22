using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WSB_Management.Migrations
{
    /// <inheritdoc />
    public partial class updatedDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Customers_TeamChefId",
                table: "Teams");

            migrationBuilder.AlterColumn<long>(
                name: "TeamChefId",
                table: "Teams",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Customers_TeamChefId",
                table: "Teams",
                column: "TeamChefId",
                principalTable: "Customers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Customers_TeamChefId",
                table: "Teams");

            migrationBuilder.AlterColumn<long>(
                name: "TeamChefId",
                table: "Teams",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Customers_TeamChefId",
                table: "Teams",
                column: "TeamChefId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
