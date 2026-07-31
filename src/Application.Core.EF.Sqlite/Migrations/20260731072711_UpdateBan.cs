using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Processed",
                table: "reports",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LinkedBanId",
                table: "macbans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LinkedBanId",
                table: "ipbans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LinkedBanId",
                table: "hwidbans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AuditAccountId",
                table: "account_ban",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Canceled",
                table: "account_ban",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OperateAccountId",
                table: "account_ban",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Processed",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "LinkedBanId",
                table: "macbans");

            migrationBuilder.DropColumn(
                name: "LinkedBanId",
                table: "ipbans");

            migrationBuilder.DropColumn(
                name: "LinkedBanId",
                table: "hwidbans");

            migrationBuilder.DropColumn(
                name: "AuditAccountId",
                table: "account_ban");

            migrationBuilder.DropColumn(
                name: "Canceled",
                table: "account_ban");

            migrationBuilder.DropColumn(
                name: "OperateAccountId",
                table: "account_ban");
        }
    }
}
