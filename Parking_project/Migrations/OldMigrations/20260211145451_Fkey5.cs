using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking_project.Migrations
{
    /// <inheritdoc />
    public partial class Fkey5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CilsimetParkimit_NjesiOrg_NjesiteId",
                table: "CilsimetParkimit");

            migrationBuilder.AddForeignKey(
                name: "FK_CilsimetParkimit_NjesiOrg_NjesiteId",
                table: "CilsimetParkimit",
                column: "NjesiteId",
                principalTable: "NjesiOrg",
                principalColumn: "NjesiteId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CilsimetParkimit_NjesiOrg_NjesiteId",
                table: "CilsimetParkimit");

            migrationBuilder.AddForeignKey(
                name: "FK_CilsimetParkimit_NjesiOrg_NjesiteId",
                table: "CilsimetParkimit",
                column: "NjesiteId",
                principalTable: "NjesiOrg",
                principalColumn: "NjesiteId");
        }
    }
}
