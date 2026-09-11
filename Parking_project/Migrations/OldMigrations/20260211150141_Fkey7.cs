using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking_project.Migrations
{
    /// <inheritdoc />
    public partial class Fkey7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NjesiOrg_Organizata_OrganizataBiznesId",
                table: "NjesiOrg");

            migrationBuilder.DropIndex(
                name: "IX_NjesiOrg_OrganizataBiznesId",
                table: "NjesiOrg");

            migrationBuilder.DropColumn(
                name: "OrganizataBiznesId",
                table: "NjesiOrg");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
