using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking_project.Migrations
{
    /// <inheritdoc />
    public partial class Fkey6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sherbimi_Organizata_BiznesId",
                table: "Sherbimi");

            migrationBuilder.DropForeignKey(
                name: "FK_TransaksionDetaj_Sherbimi_SherbimiId",
                table: "TransaksionDetaj");

            migrationBuilder.DropForeignKey(
                name: "FK_TransaksionParkimi_CilsimetParkimit_CilsimiId",
                table: "TransaksionParkimi");

            migrationBuilder.AddForeignKey(
                name: "FK_Sherbimi_Organizata_BiznesId",
                table: "Sherbimi",
                column: "BiznesId",
                principalTable: "Organizata",
                principalColumn: "BiznesId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksionDetaj_Sherbimi_SherbimiId",
                table: "TransaksionDetaj",
                column: "SherbimiId",
                principalTable: "Sherbimi",
                principalColumn: "SherbimiId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksionParkimi_CilsimetParkimit_CilsimiId",
                table: "TransaksionParkimi",
                column: "CilsimiId",
                principalTable: "CilsimetParkimit",
                principalColumn: "CilsimetiId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sherbimi_Organizata_BiznesId",
                table: "Sherbimi");

            migrationBuilder.DropForeignKey(
                name: "FK_TransaksionDetaj_Sherbimi_SherbimiId",
                table: "TransaksionDetaj");

            migrationBuilder.DropForeignKey(
                name: "FK_TransaksionParkimi_CilsimetParkimit_CilsimiId",
                table: "TransaksionParkimi");

            migrationBuilder.AddForeignKey(
                name: "FK_Sherbimi_Organizata_BiznesId",
                table: "Sherbimi",
                column: "BiznesId",
                principalTable: "Organizata",
                principalColumn: "BiznesId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksionDetaj_Sherbimi_SherbimiId",
                table: "TransaksionDetaj",
                column: "SherbimiId",
                principalTable: "Sherbimi",
                principalColumn: "SherbimiId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksionParkimi_CilsimetParkimit_CilsimiId",
                table: "TransaksionParkimi",
                column: "CilsimiId",
                principalTable: "CilsimetParkimit",
                principalColumn: "CilsimetiId");
        }
    }
}
