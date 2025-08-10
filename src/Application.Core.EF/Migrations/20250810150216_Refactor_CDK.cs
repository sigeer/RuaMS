using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class Refactor_CDK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nxcode");

            migrationBuilder.DropTable(
                name: "nxcode_items");

            migrationBuilder.AlterColumn<string>(
                name: "MAC",
                table: "account_bindings",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "''",
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30,
                oldDefaultValueSql: "''");

            migrationBuilder.CreateTable(
                name: "cdk_codes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(17)", maxLength: 17, nullable: false),
                    Expiration = table.Column<long>(type: "bigint(20) unsigned", nullable: false),
                    MaxCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cdk_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CodeId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'5'"),
                    ItemId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'4000000'"),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cdk_records",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CodeId = table.Column<int>(type: "int", nullable: false),
                    RecipientId = table.Column<int>(type: "int", nullable: false),
                    RecipientTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "idx_code",
                table: "cdk_codes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_code2",
                table: "cdk_items",
                column: "CodeId");

            migrationBuilder.CreateIndex(
                name: "idx_code1",
                table: "cdk_records",
                column: "CodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cdk_codes");

            migrationBuilder.DropTable(
                name: "cdk_items");

            migrationBuilder.DropTable(
                name: "cdk_records");

            migrationBuilder.AlterColumn<string>(
                name: "MAC",
                table: "account_bindings",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValueSql: "''",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "''");

            migrationBuilder.CreateTable(
                name: "nxcode",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    code = table.Column<string>(type: "varchar(17)", maxLength: 17, nullable: false),
                    expiration = table.Column<long>(type: "bigint(20) unsigned", nullable: false),
                    retriever = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "nxcode_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    codeid = table.Column<int>(type: "int(11)", nullable: false),
                    item = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'4000000'"),
                    quantity = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'1'"),
                    type = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'5'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "code",
                table: "nxcode",
                column: "code",
                unique: true);
        }
    }
}
