using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnused : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dueyitems");

            migrationBuilder.DropTable(
                name: "macfilters");

            migrationBuilder.DropTable(
                name: "responses");

            migrationBuilder.DropTable(
                name: "server_queue");

            migrationBuilder.DropIndex(
                name: "id1",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "world",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "lastlogin",
                table: "accounts");

            migrationBuilder.RenameIndex(
                name: "id2",
                table: "monstercarddata",
                newName: "id1");

            migrationBuilder.RenameIndex(
                name: "INVENTORYITEMID1",
                table: "inventoryequipment",
                newName: "INVENTORYITEMID");

            migrationBuilder.AddColumn<int>(
                name: "CharacterId",
                table: "petignores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "characters",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CharacterId",
                table: "petignores");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "characters");

            migrationBuilder.RenameIndex(
                name: "id1",
                table: "monstercarddata",
                newName: "id2");

            migrationBuilder.RenameIndex(
                name: "INVENTORYITEMID",
                table: "inventoryequipment",
                newName: "INVENTORYITEMID1");

            migrationBuilder.AddColumn<int>(
                name: "world",
                table: "characters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "lastlogin",
                table: "accounts",
                type: "timestamp",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "dueyitems",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    inventoryitemid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "dueyitems_ibfk_1",
                        column: x => x.PackageId,
                        principalTable: "dueypackages",
                        principalColumn: "PackageId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "macfilters",
                columns: table => new
                {
                    macfilterid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    filter = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.macfilterid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "responses",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    chat = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    response = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "server_queue",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    accountid = table.Column<int>(type: "int", nullable: false),
                    characterid = table.Column<int>(type: "int", nullable: false),
                    createTime = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    message = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<sbyte>(type: "tinyint", nullable: false),
                    value = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "id1",
                table: "characters",
                columns: new[] { "id", "accountid", "world" });

            migrationBuilder.CreateIndex(
                name: "INVENTORYITEMID",
                table: "dueyitems",
                column: "inventoryitemid");

            migrationBuilder.CreateIndex(
                name: "PackageId",
                table: "dueyitems",
                column: "PackageId");
        }
    }
}
