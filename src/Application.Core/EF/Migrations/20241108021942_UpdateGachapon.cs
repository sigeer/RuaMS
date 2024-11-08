using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGachapon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "gachapon_pool");

            migrationBuilder.DropColumn(
                name: "LevelChance",
                table: "gachapon_pool");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "gachapon_pool_item",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "gachapon_pool_level_chance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Chance = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gachapon_pool_level_chance");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "gachapon_pool_item");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "gachapon_pool",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LevelChance",
                table: "gachapon_pool",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
