using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class InitDataV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sqlDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sql");
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, "v0.0.1-data.sql")));
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, "v0.0.1-gachapon.sql")));
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, "v0.0.1-dongfangshenzhou.sql")));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 应该不需要回滚吧
        }
    }
}
