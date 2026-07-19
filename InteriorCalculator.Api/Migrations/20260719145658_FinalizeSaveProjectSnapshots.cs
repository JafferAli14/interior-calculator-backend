using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InteriorCalculator.Api.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeSaveProjectSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Projects",
                newName: "GrandTotal");

            migrationBuilder.RenameColumn(
                name: "ConfigurationJson",
                table: "Projects",
                newName: "PlannerRequestJson");

            migrationBuilder.RenameColumn(
                name: "ClientName",
                table: "Projects",
                newName: "CustomerName");

            migrationBuilder.RenameColumn(
                name: "ClientMobile",
                table: "Projects",
                newName: "CustomerPhone");

            migrationBuilder.AlterColumn<decimal>(
                name: "GrandTotal",
                table: "Projects",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AddColumn<string>(
                name: "CategorySubtotalsJson",
                table: "Projects",
                type: "longtext",
                nullable: false,
                defaultValue: "[]")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByFullName",
                table: "Projects",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByAdminId",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUsername",
                table: "Projects",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "Legacy")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Projects",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "QAR")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CustomerAddress",
                table: "Projects",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "Projects",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProjectNumber",
                table: "Projects",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Projects",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Saved")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "WarningsJson",
                table: "Projects",
                type: "longtext",
                nullable: false,
                defaultValue: "[]")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(
                "UPDATE `Projects` SET `ProjectNumber` = CONCAT('PRJ-MIG-', `Id`) WHERE `ProjectNumber` IS NULL OR `ProjectNumber` = '';");

            migrationBuilder.AlterColumn<string>(
                name: "ProjectNumber",
                table: "Projects",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectNumber",
                table: "Projects",
                column: "ProjectNumber",
                unique: true);

            migrationBuilder.CreateTable(
                name: "ProjectEstimateLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    PriceItemCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ItemName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PricingMode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Selection = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    Area = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Length = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Unit = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CustomPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Calculation = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectEstimateLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectEstimateLines_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectEstimateLines_ProjectId",
                table: "ProjectEstimateLines",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectEstimateLines");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ProjectNumber",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CategorySubtotalsJson",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedByFullName",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedByAdminId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedByUsername",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CustomerAddress",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectNumber",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "WarningsJson",
                table: "Projects");

            migrationBuilder.AlterColumn<decimal>(
                name: "GrandTotal",
                table: "Projects",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.RenameColumn(
                name: "PlannerRequestJson",
                table: "Projects",
                newName: "ConfigurationJson");

            migrationBuilder.RenameColumn(
                name: "GrandTotal",
                table: "Projects",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "CustomerPhone",
                table: "Projects",
                newName: "ClientMobile");

            migrationBuilder.RenameColumn(
                name: "CustomerName",
                table: "Projects",
                newName: "ClientName");
        }
    }
}
