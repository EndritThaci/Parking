using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking_project.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organizata",
                columns: table => new
                {
                    BiznesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmriBiznesit = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NumriUnikIdentifikues = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Adresa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NumriBiznesit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumriFiskal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumriPunetoreve = table.Column<int>(type: "int", nullable: false),
                    DataRegjistrimit = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Komuna = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefoni = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizata", x => x.BiznesId);
                });

            migrationBuilder.CreateTable(
                name: "NjesiOrg",
                columns: table => new
                {
                    NjesiteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Emri = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Kodi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Adresa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BiznesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NjesiOrg", x => x.NjesiteId);
                    table.ForeignKey(
                        name: "FK_NjesiOrg_Organizata_BiznesId",
                        column: x => x.BiznesId,
                        principalTable: "Organizata",
                        principalColumn: "BiznesId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sherbimi",
                columns: table => new
                {
                    SherbimiId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Emri = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cmimi = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    BiznesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sherbimi", x => x.SherbimiId);
                    table.ForeignKey(
                        name: "FK_Sherbimi_Organizata_BiznesId",
                        column: x => x.BiznesId,
                        principalTable: "Organizata",
                        principalColumn: "BiznesId");
                });

            migrationBuilder.CreateTable(
                name: "Useri",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Emri = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Mbiemri = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Passwordi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BiznesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Useri", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Useri_Organizata_BiznesId",
                        column: x => x.BiznesId,
                        principalTable: "Organizata",
                        principalColumn: "BiznesId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lokacioni",
                columns: table => new
                {
                    LokacioniId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kati = table.Column<int>(type: "int", nullable: false),
                    NjesiteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lokacioni", x => x.LokacioniId);
                    table.ForeignKey(
                        name: "FK_Lokacioni_NjesiOrg_NjesiteId",
                        column: x => x.NjesiteId,
                        principalTable: "NjesiOrg",
                        principalColumn: "NjesiteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CilsimetParkimit",
                columns: table => new
                {
                    CilsimetiId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Emri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NjesiteId = table.Column<int>(type: "int", nullable: false),
                    SherbimiId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CilsimetParkimit", x => x.CilsimetiId);
                    table.ForeignKey(
                        name: "FK_CilsimetParkimit_NjesiOrg_NjesiteId",
                        column: x => x.NjesiteId,
                        principalTable: "NjesiOrg",
                        principalColumn: "NjesiteId");
                    table.ForeignKey(
                        name: "FK_CilsimetParkimit_Sherbimi_SherbimiId",
                        column: x => x.SherbimiId,
                        principalTable: "Sherbimi",
                        principalColumn: "SherbimiId");
                });

            migrationBuilder.CreateTable(
                name: "Vendi",
                columns: table => new
                {
                    VendiId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendiEmri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LokacioniId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendi", x => x.VendiId);
                    table.ForeignKey(
                        name: "FK_Vendi_Lokacioni_LokacioniId",
                        column: x => x.LokacioniId,
                        principalTable: "Lokacioni",
                        principalColumn: "LokacioniId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Detajet",
                columns: table => new
                {
                    DetajetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromHour = table.Column<int>(type: "int", nullable: false),
                    ToHour = table.Column<int>(type: "int", nullable: true),
                    Cmimi = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CilsimetiId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Detajet", x => x.DetajetId);
                    table.ForeignKey(
                        name: "FK_Detajet_CilsimetParkimit_CilsimetiId",
                        column: x => x.CilsimetiId,
                        principalTable: "CilsimetParkimit",
                        principalColumn: "CilsimetiId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransaksionParkimi",
                columns: table => new
                {
                    TransaksioniId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KohaHyrjes = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KohaDaljes = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Statusi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VendiParkimitId = table.Column<int>(type: "int", nullable: false),
                    NjesiaId = table.Column<int>(type: "int", nullable: false),
                    CilsimiId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransaksionParkimi", x => x.TransaksioniId);
                    table.ForeignKey(
                        name: "FK_TransaksionParkimi_CilsimetParkimit_VendiParkimitId",
                        column: x => x.VendiParkimitId,
                        principalTable: "CilsimetParkimit",
                        principalColumn: "CilsimetiId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransaksionParkimi_NjesiOrg_NjesiaId",
                        column: x => x.NjesiaId,
                        principalTable: "NjesiOrg",
                        principalColumn: "NjesiteId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransaksionParkimi_Vendi_VendiParkimitId",
                        column: x => x.VendiParkimitId,
                        principalTable: "Vendi",
                        principalColumn: "VendiId");
                });

            migrationBuilder.CreateTable(
                name: "TransaksionDetaj",
                columns: table => new
                {
                    TransaksionId = table.Column<int>(type: "int", nullable: false),
                    SherbimiId = table.Column<int>(type: "int", nullable: false),
                    Cmimi = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransaksionDetaj", x => new { x.TransaksionId, x.SherbimiId });
                    table.ForeignKey(
                        name: "FK_TransaksionDetaj_Sherbimi_SherbimiId",
                        column: x => x.SherbimiId,
                        principalTable: "Sherbimi",
                        principalColumn: "SherbimiId");
                    table.ForeignKey(
                        name: "FK_TransaksionDetaj_TransaksionParkimi_TransaksionId",
                        column: x => x.TransaksionId,
                        principalTable: "TransaksionParkimi",
                        principalColumn: "TransaksioniId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CilsimetParkimit_NjesiteId",
                table: "CilsimetParkimit",
                column: "NjesiteId");

            migrationBuilder.CreateIndex(
                name: "IX_CilsimetParkimit_SherbimiId",
                table: "CilsimetParkimit",
                column: "SherbimiId");

            migrationBuilder.CreateIndex(
                name: "IX_Detajet_CilsimetiId",
                table: "Detajet",
                column: "CilsimetiId");

            migrationBuilder.CreateIndex(
                name: "IX_Lokacioni_NjesiteId",
                table: "Lokacioni",
                column: "NjesiteId");

            migrationBuilder.CreateIndex(
                name: "IX_NjesiOrg_BiznesId",
                table: "NjesiOrg",
                column: "BiznesId");

            migrationBuilder.CreateIndex(
                name: "IX_Sherbimi_BiznesId",
                table: "Sherbimi",
                column: "BiznesId");

            migrationBuilder.CreateIndex(
                name: "IX_TransaksionDetaj_SherbimiId",
                table: "TransaksionDetaj",
                column: "SherbimiId");

            migrationBuilder.CreateIndex(
                name: "IX_TransaksionParkimi_NjesiaId",
                table: "TransaksionParkimi",
                column: "NjesiaId");

            migrationBuilder.CreateIndex(
                name: "IX_TransaksionParkimi_VendiParkimitId",
                table: "TransaksionParkimi",
                column: "VendiParkimitId");

            migrationBuilder.CreateIndex(
                name: "IX_Useri_BiznesId",
                table: "Useri",
                column: "BiznesId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendi_LokacioniId",
                table: "Vendi",
                column: "LokacioniId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Detajet");

            migrationBuilder.DropTable(
                name: "TransaksionDetaj");

            migrationBuilder.DropTable(
                name: "Useri");

            migrationBuilder.DropTable(
                name: "TransaksionParkimi");

            migrationBuilder.DropTable(
                name: "CilsimetParkimit");

            migrationBuilder.DropTable(
                name: "Vendi");

            migrationBuilder.DropTable(
                name: "Sherbimi");

            migrationBuilder.DropTable(
                name: "Lokacioni");

            migrationBuilder.DropTable(
                name: "NjesiOrg");

            migrationBuilder.DropTable(
                name: "Organizata");
        }
    }
}
