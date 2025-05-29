using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class RefactorServer_UpdatePetId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sys_world_config");

            migrationBuilder.DropColumn(
                name: "loggedin",
                table: "accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_petignorepetid",
                table: "petignores");

            migrationBuilder.AlterColumn<ulong>(
                name: "petid",
                table: "pets",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int(11) unsigned")
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<ulong>(
                name: "petid",
                table: "petignores",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int(11) unsigned");

            migrationBuilder.AddForeignKey(
                name: "fk_petignorepetid",
                table: "petignores",
                column: "petid",
                principalTable: "pets",
                principalColumn: "petid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AlterColumn<long>(
                name: "petid",
                table: "inventoryitems",
                type: "bigint",
                nullable: false,
                defaultValueSql: "'-1'",
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldDefaultValueSql: "'-1'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_petignorepetid",
                table: "petignores");

            migrationBuilder.AlterColumn<int>(
                name: "petid",
                table: "pets",
                type: "int(11) unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "petid",
                table: "petignores",
                type: "int(11) unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned");

            migrationBuilder.AddForeignKey(
                name: "fk_petignorepetid",
                table: "petignores",
                column: "petid",
                principalTable: "pets",
                principalColumn: "petid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AlterColumn<int>(
                name: "petid",
                table: "inventoryitems",
                type: "int(11)",
                nullable: false,
                defaultValueSql: "'-1'",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "'-1'");

            migrationBuilder.AddColumn<sbyte>(
                name: "loggedin",
                table: "accounts",
                type: "tinyint(4)",
                nullable: false,
                defaultValue: (sbyte)0);

            migrationBuilder.CreateTable(
                name: "sys_world_config",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    BossDropRate = table.Column<float>(type: "float", nullable: false, defaultValue: 1f),
                    ChannelCount = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    DropRate = table.Column<float>(type: "float", nullable: false, defaultValue: 1f),
                    Enable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EventMessage = table.Column<string>(type: "varchar(200)", nullable: false, defaultValue: ""),
                    ExpRate = table.Column<float>(type: "float", nullable: false, defaultValue: 1f),
                    FishingRate = table.Column<float>(type: "float", nullable: false, defaultValue: 1f),
                    Flag = table.Column<int>(type: "int", nullable: false),
                    MesoRate = table.Column<float>(type: "float", nullable: false, defaultValue: 1f),
                    MobRate = table.Column<float>(type: "float", nullable: false, defaultValue: 1f),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false, defaultValue: ""),
                    QuestRate = table.Column<float>(type: "float", nullable: false, defaultValue: 1f),
                    RecommendMessage = table.Column<string>(type: "varchar(200)", nullable: false, defaultValue: ""),
                    ServerMessage = table.Column<string>(type: "varchar(200)", nullable: false, defaultValue: "Welcome"),
                    StartPort = table.Column<int>(type: "int", nullable: false),
                    TravelRate = table.Column<float>(type: "float", nullable: false, defaultValue: 1f)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "sys_world_config",
                columns: new[] { "Id", "BossDropRate", "ChannelCount", "DropRate", "Enable", "EventMessage", "ExpRate", "FishingRate", "Flag", "MesoRate", "MobRate", "Name", "QuestRate", "RecommendMessage", "ServerMessage", "StartPort", "TravelRate" },
                values: new object[] { 0, 10f, 3, 10f, true, "Scania!", 10f, 10f, 0, 10f, 1f, "Scania", 5f, "Welcome to Scania!", "Welcome to Scania!", 7575, 10f });
        }
    }
}
