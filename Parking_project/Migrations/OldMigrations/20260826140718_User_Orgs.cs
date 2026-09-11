using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking_project.Migrations
{
    /// <inheritdoc />
    public partial class User_Orgs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Useri_NjesiOrg_NjesiaId",
                table: "Useri");

            migrationBuilder.DropForeignKey(
                name: "FK_Useri_Organizata_BiznesId",
                table: "Useri");

            migrationBuilder.DropIndex(
                name: "IX_Useri_BiznesId",
                table: "Useri");

            migrationBuilder.DropIndex(
                name: "IX_Useri_NjesiaId",
                table: "Useri");

            migrationBuilder.DropColumn(
                name: "BiznesId",
                table: "Useri");

            migrationBuilder.DropColumn(
                name: "NjesiaId",
                table: "Useri");

            migrationBuilder.AddColumn<bool>(
                name: "AllowCustomers",
                table: "Organizata",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserOrg",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BiznesId = table.Column<int>(type: "int", nullable: false),
                    NjesiaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOrg", x => new { x.UserId, x.BiznesId });
                    table.ForeignKey(
                        name: "FK_UserOrg_NjesiOrg_NjesiaId",
                        column: x => x.NjesiaId,
                        principalTable: "NjesiOrg",
                        principalColumn: "NjesiteId");
                    table.ForeignKey(
                        name: "FK_UserOrg_Organizata_BiznesId",
                        column: x => x.BiznesId,
                        principalTable: "Organizata",
                        principalColumn: "BiznesId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserOrg_Useri_UserId",
                        column: x => x.UserId,
                        principalTable: "Useri",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserOrg_BiznesId",
                table: "UserOrg",
                column: "BiznesId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrg_NjesiaId",
                table: "UserOrg",
                column: "NjesiaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserOrg");

            migrationBuilder.DropColumn(
                name: "AllowCustomers",
                table: "Organizata");

            migrationBuilder.AddColumn<int>(
                name: "BiznesId",
                table: "Useri",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NjesiaId",
                table: "Useri",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Useri_BiznesId",
                table: "Useri",
                column: "BiznesId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Useri_Organizata_BiznesId",
                table: "Useri",
                column: "BiznesId",
                principalTable: "Organizata",
                principalColumn: "BiznesId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
