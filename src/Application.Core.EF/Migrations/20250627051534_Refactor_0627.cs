using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class Refactor_0627 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "allianceguilds");

            migrationBuilder.DropIndex(
                name: "party",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "receivername",
                table: "newyear");

            migrationBuilder.DropColumn(
                name: "sendername",
                table: "newyear");

            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "dueypackages");

            migrationBuilder.DropColumn(
                name: "messengerid",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "messengerposition",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "party",
                table: "characters");

            migrationBuilder.AddColumn<int>(
                name: "SenderId",
                table: "dueypackages",
                type: "int(10) unsigned",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderId",
                table: "dueypackages");

            migrationBuilder.AddColumn<string>(
                name: "receivername",
                table: "newyear",
                type: "varchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValueSql: "''");

            migrationBuilder.AddColumn<string>(
                name: "sendername",
                table: "newyear",
                type: "varchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValueSql: "''");

            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "dueypackages",
                type: "varchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "messengerid",
                table: "characters",
                type: "int(10) unsigned",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "messengerposition",
                table: "characters",
                type: "int(10) unsigned",
                nullable: false,
                defaultValueSql: "'4'");

            migrationBuilder.AddColumn<int>(
                name: "party",
                table: "characters",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "allianceguilds",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    allianceid = table.Column<int>(type: "int(10)", nullable: false, defaultValueSql: "'-1'"),
                    guildid = table.Column<int>(type: "int(10)", nullable: false, defaultValueSql: "'-1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "party",
                table: "characters",
                column: "party");
        }
    }
}
