using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class Refactor_Ban : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ranking1",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "banned",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "banreason",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "greason",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "hwid",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "ip",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "macs",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "tempban",
                table: "accounts");

            migrationBuilder.RenameIndex(
                name: "ranking11",
                table: "characters",
                newName: "ranking1");

            migrationBuilder.RenameIndex(
                name: "accountid",
                table: "characters",
                newName: "accountid2");

            migrationBuilder.AlterColumn<int>(
                name: "aid",
                table: "macbans",
                type: "int(10) unsigned",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "aid",
                table: "ipbans",
                type: "int(10) unsigned",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "hwidbans",
                type: "int(10) unsigned",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "account_ban",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    BanLevel = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<sbyte>(type: "tinyint(4)", nullable: false),
                    ReasonDescription = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "account_bindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    IP = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValueSql: "''"),
                    MAC = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValueSql: "''"),
                    HWID = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValueSql: "''"),
                    LastActiveTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "accountid",
                table: "account_ban",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "accountid1",
                table: "account_bindings",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_ban");

            migrationBuilder.DropTable(
                name: "account_bindings");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "hwidbans");

            migrationBuilder.RenameIndex(
                name: "ranking1",
                table: "characters",
                newName: "ranking11");

            migrationBuilder.RenameIndex(
                name: "accountid2",
                table: "characters",
                newName: "accountid");

            migrationBuilder.AlterColumn<string>(
                name: "aid",
                table: "macbans",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int(10) unsigned");

            migrationBuilder.AlterColumn<string>(
                name: "aid",
                table: "ipbans",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int(10) unsigned");

            migrationBuilder.AddColumn<sbyte>(
                name: "banned",
                table: "accounts",
                type: "tinyint",
                nullable: false,
                defaultValue: (sbyte)0);

            migrationBuilder.AddColumn<string>(
                name: "banreason",
                table: "accounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<sbyte>(
                name: "greason",
                table: "accounts",
                type: "tinyint(4)",
                nullable: false,
                defaultValue: (sbyte)0);

            migrationBuilder.AddColumn<string>(
                name: "hwid",
                table: "accounts",
                type: "varchar(12)",
                maxLength: 12,
                nullable: false,
                defaultValueSql: "''");

            migrationBuilder.AddColumn<string>(
                name: "ip",
                table: "accounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "macs",
                table: "accounts",
                type: "tinytext",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "tempban",
                table: "accounts",
                type: "timestamp",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ranking1",
                table: "accounts",
                columns: new[] { "id", "banned" });
        }
    }
}
