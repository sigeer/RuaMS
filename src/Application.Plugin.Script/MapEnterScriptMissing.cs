using Application.Core.Server.life;
using System.Drawing;

namespace Application.Plugin.Script
{
    /// <summary>
    /// 原先未移植的 37 个 <c>onUserEnter</c> 地图脚本。
    /// 这些地图在 <c>Map.wz</c> 中的 <c>onUserEnter</c> 字段为空，当前不会触发；
    /// 按「文件全覆盖」口径一并补齐，WZ 一旦引用即可生效。
    /// </summary>
    internal partial class MapEnterScript
    {
        #region 召唤守关怪物（6770000xx / 910510000 / 912000000 / 925040100 / 926000000）

        /// <summary>
        /// 地图上还没有该怪物时才召唤（JS 的 <c>if (map.getMonsterById(id) != null) return;</c>）。
        /// </summary>
        /// <param name="notice">非空时向玩家广播「XXX has appeared!」。</param>
        private async Task SpawnBossIfAbsent(int mobId, Point pos, string? notice = null)
        {
            var map = getMap();
            if (map.getMonsterById(mobId) != null)
                return;

            var mob = LifeFactory.Instance.GetMonsterTrust(mobId);
            await map.spawnMonsterOnGroundBelow(mob, pos);

            if (notice != null)
            {
                await getPlayer().TypedMessage(5, notice);
            }
        }

        // Map: 677000001
        [ScriptName("677000001")]
        public Task go677000001() => SpawnBossIfAbsent(9400612, new Point(461, 61), "Marbas 出现了！");
        // Map: 677000003
        [ScriptName("677000003")]
        public Task go677000003() => SpawnBossIfAbsent(9400610, new Point(467, 0), "Amdusias 出现了！");
        // Map: 677000005
        [ScriptName("677000005")]
        public Task go677000005() => SpawnBossIfAbsent(9400609, new Point(201, 80), "Andras 出现了！");
        // Map: 677000007
        [ScriptName("677000007")]
        public Task go677000007() => SpawnBossIfAbsent(9400611, new Point(171, 50), "Crocell 出现了！");
        // Map: 677000009
        [ScriptName("677000009")]
        public Task go677000009() => SpawnBossIfAbsent(9400613, new Point(251, -841), "Valefor 出现了！");
        // Map: 677000012
        [ScriptName("677000012")]
        public Task go677000012() => SpawnBossIfAbsent(9400633, new Point(842, 0), "Astaroth 出现了！");
        #endregion

        #region 导师 NPC 召唤（1080106xx）

        /// <summary>
        /// 五个分职业地图各自的导师 NPC：地图上还没有时才生成。
        /// JS 中 <c>npcid</c>/<c>spawnPos</c> 由调用方注入，C# 侧按地图固化。
        /// </summary>
        private async Task SpawnTutorNpc(int npcId, Point pos)
        {
            var map = getMap();
            if (map.getNPCById(npcId) != null)
                return;

            await spawnNpc(npcId, pos, map);
        }

        // Map: 108010600 / 610 / 620 / 630 / 640
        [ScriptName("108010600")]
        public Task go108010600() => SpawnTutorNpc(1104100, new Point(2830, 78));
        [ScriptName("108010610")]
        public Task go108010610() => SpawnTutorNpc(1104101, new Point(3395, -322));
        [ScriptName("108010620")]
        public Task go108010620() => SpawnTutorNpc(1104102, new Point(500, -522));
        [ScriptName("108010630")]
        public Task go108010630() => SpawnTutorNpc(1104103, new Point(-2263, -582));
        [ScriptName("108010640")]
        public Task go108010640() => SpawnTutorNpc(1104104, new Point(372, 70));

        #endregion

        // Map: 130030000, 130030001
        [ScriptName("130030000")]
        public Task go130030000() => setQuestProgress(20010, 20022, 1);
        [ScriptName("130030001")]
        public Task go130030001() => setQuestProgress(20010, 20022, 1);
    }
}
