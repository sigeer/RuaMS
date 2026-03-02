using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class Add_StoreHpMpAlert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HpAlert",
                table: "characters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MpAlert",
                table: "characters",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HpAlert",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "MpAlert",
                table: "characters");
        }
    }
}
