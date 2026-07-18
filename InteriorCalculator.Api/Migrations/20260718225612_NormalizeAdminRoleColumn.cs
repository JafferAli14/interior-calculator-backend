using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InteriorCalculator.Api.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeAdminRoleColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE `Admins` SET `Role` = 'Admin' WHERE `Role` IS NULL OR TRIM(`Role`) = '' OR `Role` NOT IN ('Admin', 'SuperAdmin');");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Admins",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Admins",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
