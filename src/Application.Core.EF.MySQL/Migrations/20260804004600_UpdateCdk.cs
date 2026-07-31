using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCdk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "idx_code2",
                table: "cdk_records",
                newName: "idx_code_records");

            migrationBuilder.RenameIndex(
                name: "idx_code1",
                table: "cdk_items",
                newName: "idx_cdk_items_code");

            migrationBuilder.RenameIndex(
                name: "idx_code",
                table: "cdk_codes",
                newName: "idx_cdk_codes_code");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "cdk_items");

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "cdk_items",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "'4000000'");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "cdk_codes",
                type: "varchar(17)",
                maxLength: 17,
                nullable: false,
                collation: "utf8mb4_bin",
                oldClrType: typeof(string),
                oldType: "varchar(17)",
                oldMaxLength: 17)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "AccountOnce",
                table: "cdk_codes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "cdk_codes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountOnce",
                table: "cdk_codes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "cdk_codes");

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "cdk_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'4000000'",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "cdk_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'5'");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "cdk_codes",
                type: "varchar(17)",
                maxLength: 17,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(17)",
                oldMaxLength: 17,
                oldCollation: "utf8mb4_bin")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.RenameIndex(
                name: "idx_code_records",
                table: "cdk_records",
                newName: "idx_code2");

            migrationBuilder.RenameIndex(
                name: "idx_cdk_items_code",
                table: "cdk_items",
                newName: "idx_code1");

            migrationBuilder.RenameIndex(
                name: "idx_cdk_codes_code",
                table: "cdk_codes",
                newName: "idx_code");
        }
    }
}
