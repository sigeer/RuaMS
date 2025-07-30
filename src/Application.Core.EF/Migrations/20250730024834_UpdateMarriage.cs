using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMarriage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "marriageItemId",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "partnerId",
                table: "characters");

            migrationBuilder.AddColumn<int>(
                name: "EngagementItemId",
                table: "marriages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RingSourceId",
                table: "marriages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "marriages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Time0",
                table: "marriages",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Time1",
                table: "marriages",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Time2",
                table: "marriages",
                type: "datetime",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EngagementItemId",
                table: "marriages");

            migrationBuilder.DropColumn(
                name: "RingSourceId",
                table: "marriages");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "marriages");

            migrationBuilder.DropColumn(
                name: "Time0",
                table: "marriages");

            migrationBuilder.DropColumn(
                name: "Time1",
                table: "marriages");

            migrationBuilder.DropColumn(
                name: "Time2",
                table: "marriages");

            migrationBuilder.AddColumn<int>(
                name: "marriageItemId",
                table: "characters",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "partnerId",
                table: "characters",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);
        }
    }
}
