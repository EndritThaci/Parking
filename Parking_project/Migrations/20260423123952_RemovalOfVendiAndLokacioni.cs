using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking_project.Migrations
{
    /// <inheritdoc />
    public partial class RemovalOfVendiAndLokacioni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransaksionParkimi_Vendi_VendiParkimitId",
                table: "TransaksionParkimi");

            migrationBuilder.DropTable(
                name: "Vendi");

            migrationBuilder.DropTable(
                name: "Lokacioni");

            migrationBuilder.DropIndex(
                name: "IX_TransaksionParkimi_VendiParkimitId",
                table: "TransaksionParkimi");

            migrationBuilder.DropColumn(
                name: "VendiParkimitId",
                table: "TransaksionParkimi");

            migrationBuilder.AddColumn<int>(
                name: "VendeTeLira",
                table: "NjesiOrg",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VendeTeLira",
                table: "NjesiOrg");

            migrationBuilder.AddColumn<int>(
                name: "VendiParkimitId",
                table: "TransaksionParkimi",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Lokacioni",
                columns: table => new
                {
                    LokacioniId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NjesiteId = table.Column<int>(type: "int", nullable: false),
                    Kati = table.Column<int>(type: "int", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
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
                name: "Vendi",
                columns: table => new
                {
                    VendiId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LokacioniId = table.Column<int>(type: "int", nullable: false),
                    IsFree = table.Column<bool>(type: "bit", nullable: false),
                    VendiEmri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_TransaksionParkimi_VendiParkimitId",
                table: "TransaksionParkimi",
                column: "VendiParkimitId");

            migrationBuilder.CreateIndex(
                name: "IX_Lokacioni_NjesiteId",
                table: "Lokacioni",
                column: "NjesiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendi_LokacioniId",
                table: "Vendi",
                column: "LokacioniId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksionParkimi_Vendi_VendiParkimitId",
                table: "TransaksionParkimi",
                column: "VendiParkimitId",
                principalTable: "Vendi",
                principalColumn: "VendiId");
        }
    }
}
