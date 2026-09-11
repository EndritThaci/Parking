using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking_project.Migrations
{
    /// <inheritdoc />
    public partial class SoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardName",
                table: "CardDetails");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "BankAccount");

            migrationBuilder.RenameColumn(
                name: "Active",
                table: "CilsimetParkimit",
                newName: "active");

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "Vendi",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "Useri",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "Sherbimi",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "NjesiOrg",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "Lokacioni",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "Detajet",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Selected",
                table: "CilsimetParkimit",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "active",
                table: "Vendi");

            migrationBuilder.DropColumn(
                name: "active",
                table: "Useri");

            migrationBuilder.DropColumn(
                name: "active",
                table: "Sherbimi");

            migrationBuilder.DropColumn(
                name: "active",
                table: "NjesiOrg");

            migrationBuilder.DropColumn(
                name: "active",
                table: "Lokacioni");

            migrationBuilder.DropColumn(
                name: "active",
                table: "Detajet");

            migrationBuilder.DropColumn(
                name: "Selected",
                table: "CilsimetParkimit");

            migrationBuilder.RenameColumn(
                name: "active",
                table: "CilsimetParkimit",
                newName: "Active");

            migrationBuilder.AddColumn<string>(
                name: "CardName",
                table: "CardDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "BankAccount",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
