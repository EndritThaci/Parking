using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking_project.Migrations
{
    /// <inheritdoc />
    public partial class Fkey3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CilsimetParkimit_Sherbimi_SherbimiId",
                table: "CilsimetParkimit");

            migrationBuilder.AddForeignKey(
                name: "FK_CilsimetParkimit_Sherbimi_SherbimiId",
                table: "CilsimetParkimit",
                column: "SherbimiId",
                principalTable: "Sherbimi",
                principalColumn: "SherbimiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CilsimetParkimit_Sherbimi_SherbimiId",
                table: "CilsimetParkimit");

            migrationBuilder.AddForeignKey(
                name: "FK_CilsimetParkimit_Sherbimi_SherbimiId",
                table: "CilsimetParkimit",
                column: "SherbimiId",
                principalTable: "Sherbimi",
                principalColumn: "SherbimiId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
