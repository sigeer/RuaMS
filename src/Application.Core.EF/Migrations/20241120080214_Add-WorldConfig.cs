using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddWorldConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "mobid",
                table: "drop_data_global",
                newName: "mobid1");

            migrationBuilder.RenameIndex(
                name: "mobid1",
                table: "drop_data",
                newName: "mobid");

            migrationBuilder.CreateTable(
                name: "sys_world_config",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false, defaultValue: ""),
                    Flag = table.Column<int>(type: "int", nullable: false),
                    Enable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EventMessage = table.Column<string>(type: "varchar(200)", nullable: false, defaultValue: ""),
                    ServerMessage = table.Column<string>(type: "varchar(200)", nullable: false, defaultValue: "Welcome"),
                    RecommendMessage = table.Column<string>(type: "varchar(200)", nullable: false, defaultValue: ""),
                    QuestRate = table.Column<float>(type: "float", nullable: false, defaultValue: 1f),
                    ExpRate = table.Column<float>(type: "float", nullable: false, defaultValue: 1f),
                    MesoRate = table.Column<float>(type: "float", nullable: false, defaultValue: 1f),
                    DropRate = table.Column<float>(type: "float", nullable: false, defaultValue: 1f),
                    BossDropRate = table.Column<float>(type: "float", nullable: false, defaultValue: 1f),
                    MobRate = table.Column<float>(type: "float", nullable: false, defaultValue: 1f),
                    FishingRate = table.Column<float>(type: "float", nullable: false, defaultValue: 1f),
                    TravelRate = table.Column<float>(type: "float", nullable: false, defaultValue: 1f),
                    ChannelCount = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    StartPort = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "sys_world_config",
                columns: new[] { "Id", "BossDropRate", "ChannelCount", "DropRate", "Enable", "EventMessage", "ExpRate", "FishingRate", "Flag", "MesoRate", "MobRate", "Name", "QuestRate", "RecommendMessage", "ServerMessage", "StartPort", "TravelRate" },
                values: new object[,]
                {
                    { 0, 10f, 3, 10f, true, "Scania!", 10f, 10f, 0, 10f, 1f, "Scania", 5f, "Welcome to Scania!", "Welcome to Scania!", 7575, 10f },
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sys_world_config");

            migrationBuilder.RenameIndex(
                name: "mobid1",
                table: "drop_data_global",
                newName: "mobid");

            migrationBuilder.RenameIndex(
                name: "mobid",
                table: "drop_data",
                newName: "mobid1");
        }
    }
}
