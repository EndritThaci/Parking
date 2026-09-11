using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking_project.Migrations
{
    /// <inheritdoc />
    public partial class TransaksionUserFKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TransaksionParkimi_UserId",
                table: "TransaksionParkimi",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksionParkimi_Useri_UserId",
                table: "TransaksionParkimi",
                column: "UserId",
                principalTable: "Useri",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransaksionParkimi_Useri_UserId",
                table: "TransaksionParkimi");

            migrationBuilder.DropIndex(
                name: "IX_TransaksionParkimi_UserId",
                table: "TransaksionParkimi");
        }
    }
}
