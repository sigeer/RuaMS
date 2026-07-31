using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSqliteAcc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "password",
                table: "accounts",
                type: "TEXT",
                maxLength: 128,
                nullable: false,
                defaultValueSql: "''",
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 128,
                oldDefaultValueSql: "''");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "accounts",
                type: "TEXT",
                maxLength: 13,
                nullable: false,
                defaultValueSql: "''",
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 13,
                oldDefaultValueSql: "''");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "accounts",
                type: "TEXT",
                maxLength: 45,
                nullable: true,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 45,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "password",
                table: "accounts",
                type: "TEXT",
                maxLength: 128,
                nullable: false,
                defaultValueSql: "''",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 128,
                oldDefaultValueSql: "''",
                oldCollation: "NOCASE");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "accounts",
                type: "TEXT",
                maxLength: 13,
                nullable: false,
                defaultValueSql: "''",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 13,
                oldDefaultValueSql: "''",
                oldCollation: "NOCASE");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "accounts",
                type: "TEXT",
                maxLength: 45,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 45,
                oldNullable: true,
                oldCollation: "NOCASE");
        }
    }
}
