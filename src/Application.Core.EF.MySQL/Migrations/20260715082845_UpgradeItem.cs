using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UniqueId",
                table: "inventoryitems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.Sql("update `inventoryitems` set `UniqueId` = `petid`;");

            migrationBuilder.DropColumn(
                name: "petid",
                table: "inventoryitems");

            migrationBuilder.AddColumn<string>(
                name: "Properties",
                table: "inventoryitems",
                type: "text",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<long>(
                name: "petid",
                table: "inventoryitems",
                type: "bigint",
                nullable: false,
                defaultValueSql: "'-1'");

            migrationBuilder.Sql("update `inventoryitems` set `petid` = `UniqueId`;");

            migrationBuilder.DropColumn(
                name: "Properties",
                table: "inventoryitems");

            migrationBuilder.DropColumn(
                name: "UniqueId",
                table: "inventoryitems");
        }
    }
}
