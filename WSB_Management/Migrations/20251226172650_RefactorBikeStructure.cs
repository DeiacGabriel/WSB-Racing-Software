using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WSB_Management.Migrations
{
    /// <inheritdoc />
    public partial class RefactorBikeStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bikes_Brands_BrandId",
                table: "Bikes");

            migrationBuilder.DropColumn(
                name: "Ccm",
                table: "Bikes");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Bikes");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Bikes");

            migrationBuilder.RenameColumn(
                name: "BrandId",
                table: "Bikes",
                newName: "BikeTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Bikes_BrandId",
                table: "Bikes",
                newName: "IX_Bikes_BikeTypeId");

            migrationBuilder.CreateTable(
                name: "Klasses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Bezeichnung = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Klasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BikeTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    BrandId = table.Column<long>(type: "bigint", nullable: false),
                    KlasseId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BikeTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BikeTypes_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BikeTypes_Klasses_KlasseId",
                        column: x => x.KlasseId,
                        principalTable: "Klasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BikeTypes_BrandId",
                table: "BikeTypes",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_BikeTypes_KlasseId",
                table: "BikeTypes",
                column: "KlasseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bikes_BikeTypes_BikeTypeId",
                table: "Bikes",
                column: "BikeTypeId",
                principalTable: "BikeTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bikes_BikeTypes_BikeTypeId",
                table: "Bikes");

            migrationBuilder.DropTable(
                name: "BikeTypes");

            migrationBuilder.DropTable(
                name: "Klasses");

            migrationBuilder.RenameColumn(
                name: "BikeTypeId",
                table: "Bikes",
                newName: "BrandId");

            migrationBuilder.RenameIndex(
                name: "IX_Bikes_BikeTypeId",
                table: "Bikes",
                newName: "IX_Bikes_BrandId");

            migrationBuilder.AddColumn<string>(
                name: "Ccm",
                table: "Bikes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Bikes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Bikes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Bikes_Brands_BrandId",
                table: "Bikes",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id");
        }
    }
}
