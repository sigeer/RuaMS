using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCoupon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nxcoupons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nxcoupons",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    activeday = table.Column<int>(type: "int", nullable: false),
                    couponid = table.Column<int>(type: "int", nullable: false),
                    endhour = table.Column<int>(type: "int", nullable: false),
                    rate = table.Column<int>(type: "int", nullable: false),
                    starthour = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                });
        }
    }
}
