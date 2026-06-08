using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InteriorCalculator.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminUsernameAndRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Admins",
                newName: "Username");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Admins",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Admins");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Admins",
                newName: "Email");
        }
    }
}
