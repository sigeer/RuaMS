using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class Refactor_HiredMerchant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "from",
                table: "notes");

            migrationBuilder.DropColumn(
                name: "to",
                table: "notes");

            migrationBuilder.DropColumn(
                name: "HasMerchant",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "MerchantMesos",
                table: "characters");

            migrationBuilder.AddColumn<int>(
                name: "fromId",
                table: "notes",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "toId",
                table: "notes",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fromId",
                table: "notes");

            migrationBuilder.DropColumn(
                name: "toId",
                table: "notes");

            migrationBuilder.AddColumn<string>(
                name: "from",
                table: "notes",
                type: "varchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValueSql: "''");

            migrationBuilder.AddColumn<string>(
                name: "to",
                table: "notes",
                type: "varchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValueSql: "''");

            migrationBuilder.AddColumn<bool>(
                name: "HasMerchant",
                table: "characters",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MerchantMesos",
                table: "characters",
                type: "int(11)",
                nullable: false,
                defaultValueSql: "'0'");
        }
    }
}
