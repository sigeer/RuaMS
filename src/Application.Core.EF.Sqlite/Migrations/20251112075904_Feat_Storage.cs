using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Feat_Storage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "accountid",
                table: "storages",
                newName: "OwnerId");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "storages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "gachapon_pool_item",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("update `gachapon_pool_item` set Quantity = 1;");
            migrationBuilder.Sql("update `gachapon_pool_item` set Quantity = 100 WHERE CAST(ItemId / 10000 AS INTEGER) = 200;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "storages");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "gachapon_pool_item");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "storages",
                newName: "accountid");
        }
    }
}
