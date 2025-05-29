using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class FixPlayerNPCAutoIncrement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 解决map的objectid重复问题
            migrationBuilder.Sql("ALTER TABLE playernpcs AUTO_INCREMENT = 2147000000;");
            // 不明
            migrationBuilder.Sql("ALTER TABLE shops AUTO_INCREMENT = 10000000;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
