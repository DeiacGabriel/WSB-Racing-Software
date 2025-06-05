using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace WSB_Management.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "brand",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brand", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "country",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Shorttxt = table.Column<string>(type: "longtext", nullable: false),
                    Longtxt = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_country", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cup",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cup", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "longtext", nullable: true),
                    Firstname = table.Column<string>(type: "longtext", nullable: false),
                    Surname = table.Column<string>(type: "longtext", nullable: false),
                    Birthdate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Sex = table.Column<string>(type: "longtext", nullable: true),
                    Mail = table.Column<string>(type: "longtext", nullable: true),
                    Phonenumber = table.Column<string>(type: "longtext", nullable: true),
                    Newsletter = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    Validfrom = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Startnumber = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    Validfrom = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Validuntil = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Vat = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Personals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    Surname = table.Column<string>(type: "longtext", nullable: false),
                    Position = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false),
                    Username = table.Column<string>(type: "longtext", nullable: false),
                    Password = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personals", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    PositionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PositionName = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.PositionId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transponder",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Number = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transponder", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bike",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Brandid = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "longtext", nullable: true),
                    Ccm = table.Column<int>(type: "int", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bike", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bike_brand_Brandid",
                        column: x => x.Brandid,
                        principalTable: "brand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "address",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Customerid = table.Column<long>(type: "bigint", nullable: false),
                    Countryid = table.Column<long>(type: "bigint", nullable: false),
                    City = table.Column<string>(type: "longtext", nullable: false),
                    Zip = table.Column<string>(type: "longtext", nullable: false),
                    Street = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_address", x => x.Id);
                    table.ForeignKey(
                        name: "FK_address_country_Countryid",
                        column: x => x.Countryid,
                        principalTable: "country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_address_customer_Customerid",
                        column: x => x.Customerid,
                        principalTable: "customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "contact",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Customerid = table.Column<long>(type: "bigint", nullable: false),
                    Firstname = table.Column<string>(type: "longtext", nullable: true),
                    Surname = table.Column<string>(type: "longtext", nullable: true),
                    Phonenumber = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contact_customer_Customerid",
                        column: x => x.Customerid,
                        principalTable: "customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "customer_cup",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Customerid = table.Column<long>(type: "bigint", nullable: false),
                    Cupid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_cup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_cup_cup_Cupid",
                        column: x => x.Cupid,
                        principalTable: "cup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_customer_cup_customer_Customerid",
                        column: x => x.Customerid,
                        principalTable: "customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "customer_bike",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Customerid = table.Column<long>(type: "bigint", nullable: false),
                    Bikeid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_bike", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_bike_bike_Bikeid",
                        column: x => x.Bikeid,
                        principalTable: "bike",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_customer_bike_customer_Customerid",
                        column: x => x.Customerid,
                        principalTable: "customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "customer_event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Customerid = table.Column<long>(type: "bigint", nullable: false),
                    Eventid = table.Column<long>(type: "bigint", nullable: false),
                    Bikeid = table.Column<long>(type: "bigint", nullable: false),
                    Transponderid = table.Column<long>(type: "bigint", nullable: false),
                    Laptime = table.Column<TimeSpan>(type: "time(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_event", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_event_bike_Bikeid",
                        column: x => x.Bikeid,
                        principalTable: "bike",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_customer_event_customer_Customerid",
                        column: x => x.Customerid,
                        principalTable: "customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_customer_event_event_Eventid",
                        column: x => x.Eventid,
                        principalTable: "event",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_customer_event_transponder_Transponderid",
                        column: x => x.Transponderid,
                        principalTable: "transponder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_address_Countryid",
                table: "address",
                column: "Countryid");

            migrationBuilder.CreateIndex(
                name: "IX_address_Customerid",
                table: "address",
                column: "Customerid");

            migrationBuilder.CreateIndex(
                name: "IX_bike_Brandid",
                table: "bike",
                column: "Brandid");

            migrationBuilder.CreateIndex(
                name: "IX_contact_Customerid",
                table: "contact",
                column: "Customerid");

            migrationBuilder.CreateIndex(
                name: "IX_customer_bike_Bikeid",
                table: "customer_bike",
                column: "Bikeid");

            migrationBuilder.CreateIndex(
                name: "IX_customer_bike_Customerid",
                table: "customer_bike",
                column: "Customerid");

            migrationBuilder.CreateIndex(
                name: "IX_customer_cup_Cupid",
                table: "customer_cup",
                column: "Cupid");

            migrationBuilder.CreateIndex(
                name: "IX_customer_cup_Customerid",
                table: "customer_cup",
                column: "Customerid");

            migrationBuilder.CreateIndex(
                name: "IX_customer_event_Bikeid",
                table: "customer_event",
                column: "Bikeid");

            migrationBuilder.CreateIndex(
                name: "IX_customer_event_Customerid",
                table: "customer_event",
                column: "Customerid");

            migrationBuilder.CreateIndex(
                name: "IX_customer_event_Eventid",
                table: "customer_event",
                column: "Eventid");

            migrationBuilder.CreateIndex(
                name: "IX_customer_event_Transponderid",
                table: "customer_event",
                column: "Transponderid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "address");

            migrationBuilder.DropTable(
                name: "contact");

            migrationBuilder.DropTable(
                name: "customer_bike");

            migrationBuilder.DropTable(
                name: "customer_cup");

            migrationBuilder.DropTable(
                name: "customer_event");

            migrationBuilder.DropTable(
                name: "Personals");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "country");

            migrationBuilder.DropTable(
                name: "cup");

            migrationBuilder.DropTable(
                name: "bike");

            migrationBuilder.DropTable(
                name: "customer");

            migrationBuilder.DropTable(
                name: "event");

            migrationBuilder.DropTable(
                name: "transponder");

            migrationBuilder.DropTable(
                name: "brand");
        }
    }
}
