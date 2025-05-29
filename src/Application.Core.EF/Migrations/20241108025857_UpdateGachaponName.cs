using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGachaponName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "gachapon_pool");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "gachapon_pool",
                type: "varchar(50)",
                nullable: false,
                defaultValueSql: "''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "gachapon_pool");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "gachapon_pool",
                type: "longtext",
                nullable: true);
        }
    }
}
