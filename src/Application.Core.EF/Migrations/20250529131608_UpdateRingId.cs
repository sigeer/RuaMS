using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRingId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "partnerRingId",
                table: "rings",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int(11)");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "rings",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int(11)")
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<long>(
                name: "ringid",
                table: "mts_items",
                type: "bigint",
                nullable: false,
                defaultValueSql: "'-1'",
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldDefaultValueSql: "'-1'");

            migrationBuilder.AlterColumn<long>(
                name: "ringid",
                table: "inventoryequipment",
                type: "bigint",
                nullable: false,
                defaultValueSql: "'-1'",
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldDefaultValueSql: "'-1'");

            migrationBuilder.AlterColumn<long>(
                name: "ringid",
                table: "gifts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int(10)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "partnerRingId",
                table: "rings",
                type: "int(11)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "rings",
                type: "int(11)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ringid",
                table: "mts_items",
                type: "int(11)",
                nullable: false,
                defaultValueSql: "'-1'",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "'-1'");

            migrationBuilder.AlterColumn<int>(
                name: "ringid",
                table: "inventoryequipment",
                type: "int(11)",
                nullable: false,
                defaultValueSql: "'-1'",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "'-1'");

            migrationBuilder.AlterColumn<int>(
                name: "ringid",
                table: "gifts",
                type: "int(10)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
