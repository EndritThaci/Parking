using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking_project.Migrations
{
    /// <inheritdoc />
    public partial class TransactionIdentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Identifikues",
                table: "TransaksionParkimi",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Identifikues",
                table: "TransaksionParkimi");
        }
    }
}
