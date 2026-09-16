using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking_project.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class QRScanner_On_Njesi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QRScanner",
                table: "Organizata");

            migrationBuilder.AddColumn<bool>(
                name: "QRScanner",
                table: "NjesiOrg",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QRScanner",
                table: "NjesiOrg");

            migrationBuilder.AddColumn<bool>(
                name: "QRScanner",
                table: "Organizata",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
