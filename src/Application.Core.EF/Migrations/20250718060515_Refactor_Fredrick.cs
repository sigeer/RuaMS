using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class Refactor_Fredrick : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventorymerchant");

            migrationBuilder.RenameIndex(
                name: "INVENTORYITEMID2",
                table: "inventoryequipment",
                newName: "INVENTORYITEMID1");

            migrationBuilder.AddColumn<long>(
                name: "itemMeso",
                table: "fredstorage",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "meso",
                table: "fredstorage",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "itemMeso",
                table: "fredstorage");

            migrationBuilder.DropColumn(
                name: "meso",
                table: "fredstorage");

            migrationBuilder.RenameIndex(
                name: "INVENTORYITEMID1",
                table: "inventoryequipment",
                newName: "INVENTORYITEMID2");

            migrationBuilder.CreateTable(
                name: "inventorymerchant",
                columns: table => new
                {
                    inventorymerchantid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    bundles = table.Column<int>(type: "int(10)", nullable: false),
                    characterid = table.Column<int>(type: "int(11)", nullable: true),
                    inventoryitemid = table.Column<int>(type: "int(10) unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.inventorymerchantid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "INVENTORYITEMID1",
                table: "inventorymerchant",
                column: "inventoryitemid");
        }
    }
}
