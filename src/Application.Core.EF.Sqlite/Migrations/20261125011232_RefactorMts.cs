using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class RefactorMts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "acc",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "avoid",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "dex",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "expiration",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "flag",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "giftFrom",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "hands",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "hp",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "int",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "isequip",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "itemexp",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "itemid",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "itemlevel",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "jump",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "locked",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "luk",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "matk",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "mdef",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "mp",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "owner",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "position",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "ringid",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "sellername",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "speed",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "str",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "upgradeslots",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "vicious",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "watk",
                table: "mts_items");

            migrationBuilder.RenameColumn(
                name: "level",
                table: "mts_items",
                newName: "bidder");

            migrationBuilder.RenameColumn(
                name: "wdef",
                table: "mts_items",
                newName: "bid_count");

            migrationBuilder.AlterColumn<long>(
                name: "sell_ends",
                table: "mts_items",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 16);

            migrationBuilder.AddColumn<byte[]>(
                name: "item_blob",
                table: "mts_items",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "release_date",
                table: "mts_items",
                type: "bigint",
                nullable: false,
                defaultValueSql: "'0'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "item_blob",
                table: "mts_items");

            migrationBuilder.DropColumn(
                name: "release_date",
                table: "mts_items");

            migrationBuilder.RenameColumn(
                name: "bidder",
                table: "mts_items",
                newName: "level");

            migrationBuilder.RenameColumn(
                name: "bid_count",
                table: "mts_items",
                newName: "wdef");

            migrationBuilder.AlterColumn<string>(
                name: "sell_ends",
                table: "mts_items",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "acc",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "avoid",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "dex",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<long>(
                name: "expiration",
                table: "mts_items",
                type: "bigint",
                nullable: false,
                defaultValueSql: "'-1'");

            migrationBuilder.AddColumn<int>(
                name: "flag",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "giftFrom",
                table: "mts_items",
                type: "TEXT",
                maxLength: 26,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "hands",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "hp",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "int",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "isequip",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "itemexp",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "itemid",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "itemlevel",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'1'");

            migrationBuilder.AddColumn<int>(
                name: "jump",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "locked",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "luk",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "matk",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "mdef",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "mp",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<string>(
                name: "owner",
                table: "mts_items",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValueSql: "''");

            migrationBuilder.AddColumn<int>(
                name: "position",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "quantity",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'1'");

            migrationBuilder.AddColumn<long>(
                name: "ringid",
                table: "mts_items",
                type: "bigint",
                nullable: false,
                defaultValueSql: "'-1'");

            migrationBuilder.AddColumn<string>(
                name: "sellername",
                table: "mts_items",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "speed",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "str",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "upgradeslots",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "vicious",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "watk",
                table: "mts_items",
                type: "int",
                nullable: false,
                defaultValueSql: "'0'");
        }
    }
}
