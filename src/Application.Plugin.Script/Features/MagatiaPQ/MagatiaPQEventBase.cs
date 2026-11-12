using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.model;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using System.Drawing;

namespace Application.Plugin.Script.Features.MagatiaPQ
{
    /// <summary>
    /// 玛加提亚组队任务的公共部分。对应 JS <c>scripts/event/MagatiaPQ_A.js</c>（Alcadno）与
    /// <c>MagatiaPQ_Z.js</c>（Zenumist）——两套脚本只有地图号基数与角色分支不同，其余逻辑一致。
    ///
    /// 流程：调查 26 个书柜拿到信 → 三只烧瓶开门 → 两条支线取文件 → 开门 → 训练塔爬塔 →
    /// 犹泰办公室触发伏击 → 击破 Frankenroid 通关。
    /// </summary>
    internal abstract class MagatiaPQEventBase : AbstractPartyQuestEventTemplate
    {
        /// <summary>犹泰办公室伏击刷出的两种小怪。</summary>
        internal const int OfficeMob1 = 9300143;
        internal const int OfficeMob2 = 9300144;

        /// <summary>犹泰办公室地图相对基数偏移。</summary>
        internal const int OfficeOffset = 203;

        /// <summary>负责发放通关奖励的 NPC（朱丽叶 / 罗密欧）。</summary>
        internal const int RewardNpc = 2112018;

        /// <summary>本分支的副本地图基数。</summary>
        internal abstract int MapBase { get; }

        /// <summary>本分支是否为 Alcadno（否则 Zenumist）。</summary>
        internal abstract bool IsAlcadno { get; }

        /// <summary>犹泰办公室的 NPC（触发伏击的那一位）。</summary>
        internal abstract int YuleteOfficeNpc { get; }

        /// <summary>犹泰「被打败」后的 NPC。</summary>
        internal abstract int YuleteDeadNpc { get; }

        /// <summary>本分支的护送对象 NPC（朱丽叶 / 罗密欧）。</summary>
        internal abstract int EscortNpc { get; }

        /// <summary>本分支的弱 / 强 Frankenroid。</summary>
        internal abstract int WeakBossId { get; }
        internal abstract int StrongBossId { get; }

        /// <summary>出口反应堆名字（<c>rnj6_out</c> / <c>jnr6_out</c>）。</summary>
        internal abstract string ExitReactorName { get; }

        protected MagatiaPQEventBase(string name, int mapBase) : base(name)
        {
            MinCount = 4;
            MaxCount = 4;
            MinLevel = 71;
            MaxLevel = 85;

            EntryMap = mapBase;
            ExitMap = mapBase + 700;
            ClearMap = mapBase + 700;
            MinMap = mapBase;
            MaxMap = mapBase + 600;

            EventTime = 45 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;

            AllClearRewards = new()
            {
                {
                    1,
                    new RewardPools(
                        [
                            new(2000003, 100),
                            new(2000002, 100),
                            new(2000004, 20),
                            new(2000005, 10),
                            new(2022003, 50),
                            new(1032016, 1),
                            new(1032015, 1),
                            new(1032014, 1),
                            new(2041212, 1),
                            new(2041020, 1),
                            new(2040502, 1),
                            new(2041016, 1),
                            new(2044701, 1),
                            new(2040301, 1),
                            new(2043201, 1),
                            new(2040501, 1),
                            new(2040704, 1),
                            new(2044001, 1),
                            new(2043701, 1),
                            new(2040803, 1),
                            new(1102026, 1),
                            new(1102028, 1),
                            new(1102029, 1),
                        ],
                        [],
                        []
                    )
                },
            };

            // JS 的 setEventClearStageExp：[0, 10000, 20000, 0, 20000, 20000, 0, 0]，
            // 下标 0 对应「第 1 关」。这里以关卡号为 key（GiveStageClearRewardAll 只把它当查表 key，
            // 不要求是地图 id）——有意如此：第 1 关与第 2 关可能落在同一张地图（base+1），
            // 若改用地图 id，第 1 关领奖后会把第 2 关的奖励一并挡掉。
            StageClearRewards = new()
            {
                { 1, new(0, 0) },
                { 2, new(10_000, 0) },
                { 3, new(20_000, 0) },
                { 4, new(0, 0) },
                { 5, new(20_000, 0) },
                { 6, new(20_000, 0) },
                { 7, new(0, 0) },
            };
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)        {
            eim.setIntProperty("isAlcadno", IsAlcadno ? 1 : 0);

            eim.setIntProperty("escortFail", 0);
            eim.setIntProperty("yuleteTimeout", 0);
            eim.setIntProperty("yuleteTalked", 0);
            eim.setIntProperty("yuletePassed", 0);
            eim.setIntProperty("npcShocked", 0);
            eim.setIntProperty("normalClear", 0);

            eim.setIntProperty("statusStg1", 0);
            eim.setIntProperty("statusStg2", 0);
            eim.setIntProperty("statusStg3", 0);
            eim.setIntProperty("statusStg4", 0);
            eim.setIntProperty("statusStg5", 0);
            eim.setIntProperty("statusStg6", 0);
            eim.setIntProperty("statusStg7", 0);


            await eim.spawnNpc(YuleteOfficeNpc, new Point(252, 243), await eim.getMapInstance(MapBase + OfficeOffset));
            await eim.spawnNpc(YuleteOfficeNpc, new Point(200, 100), await eim.getMapInstance(MapBase + 401));
            await eim.spawnNpc(YuleteDeadNpc, new Point(200, 100), await eim.getMapInstance(MapBase + 500));
            await eim.spawnNpc(RewardNpc, new Point(200, 100), await eim.getMapInstance(MapBase + 600));

            await base.OnSetup(eim, level, lobbyId);
        }

        public override async Task AfterSeup(AbstractEventInstanceManager eim)
        {
            await base.AfterSeup(eim);

            // 第一阶段柜子：5 个空 + 7 金币 + 7 经验 + 1 封信 + 1 个触发器，随机打乱
            int[] books = [-1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, 1, 1, 1, 1, 1, 1, 1, 2, 3];
            Random.Shared.Shuffle(books);

            for (var i = 0; i < books.Length; i++)
            {
                eim.setIntProperty("stg1_b" + i, books[i]);
            }
        }

        public override async Task OnPlayerMapChanged(AbstractEventInstanceManager eim, Player player, int mapid)
        {
            // 有人进办公室后 10 秒，犹泰发动伏击
            if (mapid == MapBase + OfficeOffset && eim.getIntProperty("yuleteTimeout") == 0)
            {
                eim.setIntProperty("yuleteTimeout", 1);
                eim.Schedule(YuleteAction, 10_000);
            }

            await base.OnPlayerMapChanged(eim, player, mapid);
        }

        async Task YuleteAction(AbstractEventInstanceManager eim)
        {
            if (eim.getIntProperty("yuleteTalked") == 1)
            {
                eim.setIntProperty("yuletePassed", 1);
                await eim.dropMessage(5, "犹泰：呃...你们真让我作呕。我想要的不过是让这个国家成为全世界最伟大的炼金术强国。如果他们不接受，我不惜一切代价也要独自实现这个目标！！！");
            }
            else
            {
                await eim.dropMessage(5, "犹泰：哈哈哈...你们真以为我会毫无准备，不知道玛加提亚协会的走狗们会来追捕我吗？愚蠢！");
            }
            eim.setIntProperty("yuleteTalked", -1);

            var map = await eim.getMapInstance(MapBase + OfficeOffset);
            await map.destroyNPC(YuleteOfficeNpc);

            foreach (var x in new[] { -455, 0, 360 })
            {
                for (var i = 0; i < 5; i++)
                {
                    await map.spawnMonsterOnGroundBelow(OfficeMob1, x, 135);
                    await map.spawnMonsterOnGroundBelow(OfficeMob2, x, 135);
                }
            }
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            var map = mob.getMap();
            var mapId = map.getId();

            if (mapId == MapBase + 1 && eim.getIntProperty("statusStg1") == 1)
            {
                if (map.countMonsters() == 0)
                {
                    await eim.showClearEffect(mapId);
                    await eim.GiveStageClearRewardAll(2);
                    eim.setIntProperty("statusStg2", 1);
                }
            }
            else if (mapId == MapBase + OfficeOffset && eim.getIntProperty("statusStg1") == 1)
            {
                if (map.countMonsters() == 0)
                {
                    await eim.showClearEffect(mapId);
                    await eim.GiveStageClearRewardAll(5);

                    GenerateStg6Combo(eim);

                    var reactor = map.getReactorByName(ExitReactorName);
                    if (reactor != null)
                    {
                        await reactor.forceHitReactor(1);
                    }
                }
            }
            else if (mob.getId() == WeakBossId || mob.getId() == StrongBossId)
            {
                await eim.showClearEffect(mapId);

                // 第 7 关的 expStages 为 0，这里不需要发阶段奖励

                // 通关后把朱丽叶 / 罗密欧放到奖励地图
                await eim.spawnNpc(EscortNpc, new Point(-370, -150), map);

                // JS 里这条判定写的是 9300139（Zenumist 的弱 boss），对 Alcadno 分支其实是笔误，
                // 这里照搬以保持与线上一致。
                var gain = eim.getIntProperty("escortFail") == 1
                    ? 90000
                    : (mob.getId() == 9300139 ? 105000 : 140000);
                await eim.giveEventPlayersExp(gain);

                await map.killAllMonstersNotFriendly();

                if (mob.getId() == 9300139)
                {
                    eim.setIntProperty("normalClear", 1);
                }

                await eim.clearPQ();
            }

            await base.OnMobKilled(eim, mob, killer);
        }

        public override async Task OnFriendlyMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            // 护送对象被打死 → 记录失败（通关经验从 140000 降到 90000）
            eim.setIntProperty("escortFail", 1);

            await base.OnFriendlyMobKilled(eim, mob, killer);
        }

        /// <summary>第六阶段爬塔：为 4 条队伍各生成一串 10 位、每层 0~3 的组合。</summary>
        static void GenerateStg6Combo(AbstractEventInstanceManager eim)
        {
            var matrix = new int[4, 10];
            for (var j = 0; j < 10; j++)
            {
                int[] array = [0, 1, 2, 3];
                Random.Shared.Shuffle(array);

                for (var i = 0; i < 4; i++)
                {
                    matrix[i, j] = array[i];
                }
            }

            for (var i = 0; i < 4; i++)
            {
                var comb = new System.Text.StringBuilder(10);
                for (var j = 0; j < 10; j++)
                {
                    comb.Append(matrix[i, j]);
                }
                eim.setProperty("stage6_comb" + (i + 1), comb.ToString());
            }
        }
    }
}
