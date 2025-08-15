using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class InitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sqlDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SqliteSeedData");
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, "v0.0.1-data_admin.sql")));
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, "v0.0.1-data_maker.sql")));
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, "v0.0.1-data_mob_drop.sql")));
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, "v0.0.1-data_reactor_drop.sql")));
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, "v0.0.1-data_shop.sql")));
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, "v0.0.1-data_other.sql")));
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, "v0.0.1-gachapon.sql")));
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(sqlDir, "v0.0.1-dongfangshenzhou.sql")));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
