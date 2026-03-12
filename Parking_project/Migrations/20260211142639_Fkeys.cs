using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking_project.Migrations
{
    /// <inheritdoc />
    public partial class Fkeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NjesiOrg_Organizata_BiznesId",
                table: "NjesiOrg");

            migrationBuilder.AddForeignKey(
                name: "FK_NjesiOrg_Organizata_BiznesId",
                table: "NjesiOrg",
                column: "BiznesId",
                principalTable: "Organizata",
                principalColumn: "BiznesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NjesiOrg_Organizata_BiznesId",
                table: "NjesiOrg");

            migrationBuilder.AddForeignKey(
                name: "FK_NjesiOrg_Organizata_BiznesId",
                table: "NjesiOrg",
                column: "BiznesId",
                principalTable: "Organizata",
                principalColumn: "BiznesId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
