using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class removeworldtransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "worldtransfers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "worldtransfers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    characterid = table.Column<int>(type: "int(11)", nullable: false),
                    completionTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: true),
                    from = table.Column<sbyte>(type: "tinyint(3)", nullable: false),
                    requestTime = table.Column<DateTimeOffset>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    to = table.Column<sbyte>(type: "tinyint(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "characterid2",
                table: "worldtransfers",
                column: "characterid");
        }
    }
}
