using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ranking2",
                table: "characters");

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "sys_world_config",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DropColumn(
                name: "mute",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "rewardpoints",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "sitelogged",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "votepoints",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "webadmin",
                table: "accounts");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "tempban",
                table: "accounts",
                type: "timestamp",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp",
                oldDefaultValueSql: "'2005-05-11 00:00:00'");

            migrationBuilder.AddColumn<sbyte>(
                name: "gmlevel",
                table: "accounts",
                type: "tinyint",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.Sql(@"UPDATE accounts acc
                JOIN (
                    SELECT accountid, MAX(gm) AS max_gm
                    FROM characters
                    GROUP BY accountid
                ) ch ON acc.id = ch.accountid
                SET acc.gmlevel = ch.max_gm;
                ");

            migrationBuilder.DropColumn(
                name: "gm",
                table: "characters");


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "gmlevel",
                table: "accounts");

            migrationBuilder.AddColumn<sbyte>(
                name: "gm",
                table: "characters",
                type: "tinyint",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "tempban",
                table: "accounts",
                type: "timestamp",
                nullable: false,
                defaultValueSql: "'2005-05-11 00:00:00'",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "mute",
                table: "accounts",
                type: "int(1)",
                nullable: true,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "rewardpoints",
                table: "accounts",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "sitelogged",
                table: "accounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "votepoints",
                table: "accounts",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "webadmin",
                table: "accounts",
                type: "int(1)",
                nullable: true,
                defaultValueSql: "'0'");

            migrationBuilder.InsertData(
                table: "sys_world_config",
                columns: new[] { "Id", "BossDropRate", "ChannelCount", "DropRate", "Enable", "EventMessage", "ExpRate", "FishingRate", "Flag", "MesoRate", "MobRate", "Name", "QuestRate", "RecommendMessage", "ServerMessage", "StartPort", "TravelRate" },
                values: new object[,]
                {
                    { 1, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Bera", 1f, "", "Welcome", 0, 1f },
                    { 2, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Broa", 1f, "", "Welcome", 0, 1f },
                    { 3, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Windia", 1f, "", "Welcome", 0, 1f },
                    { 4, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Khaini", 1f, "", "Welcome", 0, 1f },
                    { 5, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Bellocan", 1f, "", "Welcome", 0, 1f },
                    { 6, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Mardia", 1f, "", "Welcome", 0, 1f },
                    { 7, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Kradia", 1f, "", "Welcome", 0, 1f },
                    { 8, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Yellonde", 1f, "", "Welcome", 0, 1f },
                    { 9, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Demethos", 1f, "", "Welcome", 0, 1f },
                    { 10, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Galicia", 1f, "", "Welcome", 0, 1f },
                    { 11, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "El Nido", 1f, "", "Welcome", 0, 1f },
                    { 12, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Zenith", 1f, "", "Welcome", 0, 1f },
                    { 13, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Arcenia", 1f, "", "Welcome", 0, 1f },
                    { 14, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Kastia", 1f, "", "Welcome", 0, 1f },
                    { 15, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Judis", 1f, "", "Welcome", 0, 1f },
                    { 16, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Plana", 1f, "", "Welcome", 0, 1f },
                    { 17, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Kalluna", 1f, "", "Welcome", 0, 1f },
                    { 18, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Stius", 1f, "", "Welcome", 0, 1f },
                    { 19, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Croa", 1f, "", "Welcome", 0, 1f },
                    { 20, 1f, 3, 1f, false, "", 1f, 1f, 0, 1f, 1f, "Medere", 1f, "", "Welcome", 0, 1f }
                });

            migrationBuilder.CreateIndex(
                name: "ranking2",
                table: "characters",
                columns: new[] { "gm", "job" });
        }
    }
}
