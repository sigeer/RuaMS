using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class MinorUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bosslog_daily");

            migrationBuilder.DropTable(
                name: "bosslog_weekly");

            migrationBuilder.RenameColumn(
                name: "PackageId",
                table: "dueypackages",
                newName: "Id");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClaimTime",
                table: "gifts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "boss_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    BossType = table.Column<string>(type: "varchar(20)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Flag = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "boss_log");

            migrationBuilder.DropColumn(
                name: "ClaimTime",
                table: "gifts");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "dueypackages",
                newName: "PackageId");

            migrationBuilder.CreateTable(
                name: "bosslog_daily",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    attempttime = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    bosstype = table.Column<string>(type: "enum('ZAKUM','HORNTAIL','PINKBEAN','SCARGA','PAPULATUS')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    characterid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bosslog_weekly",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    attempttime = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    bosstype = table.Column<string>(type: "enum('ZAKUM','HORNTAIL','PINKBEAN','SCARGA','PAPULATUS')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    characterid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
