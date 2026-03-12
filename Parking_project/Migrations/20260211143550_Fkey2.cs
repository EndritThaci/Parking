using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking_project.Migrations
{
    /// <inheritdoc />
    public partial class Fkey2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CilsimetParkimit_Sherbimi_SherbimiId",
                table: "CilsimetParkimit");

            migrationBuilder.DropForeignKey(
                name: "FK_NjesiOrg_Organizata_BiznesId",
                table: "NjesiOrg");

            migrationBuilder.AddColumn<int>(
                name: "OrganizataBiznesId",
                table: "Sherbimi",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sherbimi_OrganizataBiznesId",
                table: "Sherbimi",
                column: "OrganizataBiznesId");

            migrationBuilder.AddForeignKey(
                name: "FK_CilsimetParkimit_Sherbimi_SherbimiId",
                table: "CilsimetParkimit",
                column: "SherbimiId",
                principalTable: "Sherbimi",
                principalColumn: "SherbimiId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NjesiOrg_Organizata_BiznesId",
                table: "NjesiOrg",
                column: "BiznesId",
                principalTable: "Organizata",
                principalColumn: "BiznesId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sherbimi_Organizata_OrganizataBiznesId",
                table: "Sherbimi",
                column: "OrganizataBiznesId",
                principalTable: "Organizata",
                principalColumn: "BiznesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CilsimetParkimit_Sherbimi_SherbimiId",
                table: "CilsimetParkimit");

            migrationBuilder.DropForeignKey(
                name: "FK_NjesiOrg_Organizata_BiznesId",
                table: "NjesiOrg");

            migrationBuilder.DropForeignKey(
                name: "FK_Sherbimi_Organizata_OrganizataBiznesId",
                table: "Sherbimi");

            migrationBuilder.DropIndex(
                name: "IX_Sherbimi_OrganizataBiznesId",
                table: "Sherbimi");

            migrationBuilder.DropColumn(
                name: "OrganizataBiznesId",
                table: "Sherbimi");

            migrationBuilder.AddForeignKey(
                name: "FK_CilsimetParkimit_Sherbimi_SherbimiId",
                table: "CilsimetParkimit",
                column: "SherbimiId",
                principalTable: "Sherbimi",
                principalColumn: "SherbimiId");

            migrationBuilder.AddForeignKey(
                name: "FK_NjesiOrg_Organizata_BiznesId",
                table: "NjesiOrg",
                column: "BiznesId",
                principalTable: "Organizata",
                principalColumn: "BiznesId");
        }
    }
}
