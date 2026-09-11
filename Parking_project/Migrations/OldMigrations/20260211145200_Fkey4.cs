using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking_project.Migrations
{
    /// <inheritdoc />
    public partial class Fkey4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sherbimi_Organizata_OrganizataBiznesId",
                table: "Sherbimi");

            migrationBuilder.DropForeignKey(
                name: "FK_TransaksionParkimi_CilsimetParkimit_CilsimiId",
                table: "TransaksionParkimi");

            migrationBuilder.DropForeignKey(
                name: "FK_TransaksionParkimi_NjesiOrg_NjesiaId",
                table: "TransaksionParkimi");

            migrationBuilder.DropIndex(
                name: "IX_Sherbimi_OrganizataBiznesId",
                table: "Sherbimi");

            migrationBuilder.DropColumn(
                name: "OrganizataBiznesId",
                table: "Sherbimi");

            migrationBuilder.AddColumn<int>(
                name: "OrganizataBiznesId",
                table: "NjesiOrg",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NjesiOrg_OrganizataBiznesId",
                table: "NjesiOrg",
                column: "OrganizataBiznesId");

            migrationBuilder.AddForeignKey(
                name: "FK_NjesiOrg_Organizata_OrganizataBiznesId",
                table: "NjesiOrg",
                column: "OrganizataBiznesId",
                principalTable: "Organizata",
                principalColumn: "BiznesId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksionParkimi_CilsimetParkimit_CilsimiId",
                table: "TransaksionParkimi",
                column: "CilsimiId",
                principalTable: "CilsimetParkimit",
                principalColumn: "CilsimetiId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksionParkimi_NjesiOrg_NjesiaId",
                table: "TransaksionParkimi",
                column: "NjesiaId",
                principalTable: "NjesiOrg",
                principalColumn: "NjesiteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NjesiOrg_Organizata_OrganizataBiznesId",
                table: "NjesiOrg");

            migrationBuilder.DropForeignKey(
                name: "FK_TransaksionParkimi_CilsimetParkimit_CilsimiId",
                table: "TransaksionParkimi");

            migrationBuilder.DropForeignKey(
                name: "FK_TransaksionParkimi_NjesiOrg_NjesiaId",
                table: "TransaksionParkimi");

            migrationBuilder.DropIndex(
                name: "IX_NjesiOrg_OrganizataBiznesId",
                table: "NjesiOrg");

            migrationBuilder.DropColumn(
                name: "OrganizataBiznesId",
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
                name: "FK_Sherbimi_Organizata_OrganizataBiznesId",
                table: "Sherbimi",
                column: "OrganizataBiznesId",
                principalTable: "Organizata",
                principalColumn: "BiznesId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksionParkimi_CilsimetParkimit_CilsimiId",
                table: "TransaksionParkimi",
                column: "CilsimiId",
                principalTable: "CilsimetParkimit",
                principalColumn: "CilsimetiId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksionParkimi_NjesiOrg_NjesiaId",
                table: "TransaksionParkimi",
                column: "NjesiaId",
                principalTable: "NjesiOrg",
                principalColumn: "NjesiteId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
