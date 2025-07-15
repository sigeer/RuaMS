using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Application.Core.EF.Migrations
{
    /// <inheritdoc />
    public partial class Update_Gift_Ring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 删除旧的列
            migrationBuilder.Sql("ALTER TABLE `rings` DROP COLUMN `partnername`;");
            migrationBuilder.Sql("ALTER TABLE `gifts` DROP COLUMN `from`;");
            migrationBuilder.Sql("ALTER TABLE `gifts` DROP COLUMN `ringid`;");

            // 重命名列
            migrationBuilder.Sql("ALTER TABLE `rings` CHANGE `partnerRingId` `ringId2` BIGINT NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE `rings` CHANGE `partnerChrId` `characterId2` INT(11) NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE `gifts` CHANGE `to` `toId` INT(11) NOT NULL;");

            // 修改 id 列的类型（修改为 INT 并保留 AUTO_INCREMENT）
            migrationBuilder.Sql("ALTER TABLE `rings` MODIFY COLUMN `id` INT(11) NOT NULL AUTO_INCREMENT;");

            // 添加新列
            migrationBuilder.Sql("ALTER TABLE `rings` ADD COLUMN `characterId1` INT(11) NOT NULL DEFAULT 0;");
            migrationBuilder.Sql("ALTER TABLE `rings` ADD COLUMN `ringId1` BIGINT NOT NULL DEFAULT 0;");
            migrationBuilder.Sql("ALTER TABLE `gifts` ADD COLUMN `fromId` INT(11) NOT NULL DEFAULT 0;");
            migrationBuilder.Sql("ALTER TABLE `gifts` ADD COLUMN `ringSourceId` INT(11) NOT NULL DEFAULT 0;");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 删除新增字段
            migrationBuilder.Sql("ALTER TABLE `rings` DROP COLUMN `characterId1`;");
            migrationBuilder.Sql("ALTER TABLE `rings` DROP COLUMN `ringId1`;");
            migrationBuilder.Sql("ALTER TABLE `gifts` DROP COLUMN `fromId`;");
            migrationBuilder.Sql("ALTER TABLE `gifts` DROP COLUMN `ringSourceId`;");

            // 重命名列回去
            migrationBuilder.Sql("ALTER TABLE `rings` CHANGE `ringId2` `partnerRingId` BIGINT NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE `rings` CHANGE `characterId2` `partnerChrId` INT(11) NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE `gifts` CHANGE `toId` `to` INT(11) NOT NULL;");

            // 恢复 id 类型回 bigint（保留自增）
            migrationBuilder.Sql("ALTER TABLE `rings` MODIFY COLUMN `id` BIGINT NOT NULL AUTO_INCREMENT;");

            // 恢复被删除的列
            migrationBuilder.Sql("ALTER TABLE `rings` ADD COLUMN `partnername` VARCHAR(255) NOT NULL DEFAULT '';");
            migrationBuilder.Sql("ALTER TABLE `gifts` ADD COLUMN `from` VARCHAR(13) NOT NULL DEFAULT '';");
            migrationBuilder.Sql("ALTER TABLE `gifts` ADD COLUMN `ringid` BIGINT NOT NULL DEFAULT 0;");
        }

    }
}
