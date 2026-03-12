using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking_project.Migrations
{
    /// <inheritdoc />
    public partial class UserNjesi_Manager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NjesiaId",
                table: "Useri",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Useri_NjesiaId",
                table: "Useri",
                column: "NjesiaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Useri_NjesiOrg_NjesiaId",
                table: "Useri",
                column: "NjesiaId",
                principalTable: "NjesiOrg",
                principalColumn: "NjesiteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Useri_NjesiOrg_NjesiaId",
                table: "Useri");

            migrationBuilder.DropIndex(
                name: "IX_Useri_NjesiaId",
                table: "Useri");

            migrationBuilder.DropColumn(
                name: "NjesiaId",
                table: "Useri");
        }
    }
}
