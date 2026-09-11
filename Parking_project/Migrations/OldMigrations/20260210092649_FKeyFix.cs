using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking_project.Migrations
{
    /// <inheritdoc />
    public partial class FKeyFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransaksionParkimi_CilsimetParkimit_VendiParkimitId",
                table: "TransaksionParkimi");

            migrationBuilder.CreateIndex(
                name: "IX_TransaksionParkimi_CilsimiId",
                table: "TransaksionParkimi",
                column: "CilsimiId");

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
                name: "FK_TransaksionParkimi_CilsimetParkimit_CilsimiId",
                table: "TransaksionParkimi");

            migrationBuilder.DropIndex(
                name: "IX_TransaksionParkimi_CilsimiId",
                table: "TransaksionParkimi");

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksionParkimi_CilsimetParkimit_VendiParkimitId",
                table: "TransaksionParkimi",
                column: "VendiParkimitId",
                principalTable: "CilsimetParkimit",
                principalColumn: "CilsimetiId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
