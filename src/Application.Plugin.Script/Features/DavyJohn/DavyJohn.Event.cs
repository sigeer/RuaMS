using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using System.Drawing;

namespace Application.Plugin.Script.Features.DavyJohn
{
    /// <summary>
    /// 海盗船（Davy John）组队任务。对应 JS <c>scripts/event/PiratePQ.js</c>。
    ///
    /// 流程：甲板 → 击杀守卫开启传送门 → 两段过道（风花房）→ 船长室 → 击破海盗领主。
    /// 每个阶段进入时重置计时器（6/6/6/6/8 分钟）。
    /// </summary>
    internal sealed class Event : AbstractPartyQuestEventTemplate
    {
        /// <summary>过道房间：怪物自生自灭，不参与 clear 特效判定。</summary>
        static readonly int[] PassageMaps = [DavyJohnFeature.Stage3Sub2, DavyJohnFeature.Stage4Sub2];

        /// <summary>海盗领主（击杀后释放「无恙」NPC）。</summary>
        static readonly int[] LordPirates = [9300105, 9300106, 9300107, 9300119];

        public Event() : base(DavyJohnFeature.EventName)
        {
            QuestId = 1204;

            MinCount = 3;
            MaxCount = 6;
            MinLevel = 55;
            MaxLevel = 100;

            EntryMap = DavyJohnFeature.Stage1Map;
            ExitMap = DavyJohnFeature.ExitMapId;
            RecruitMap = DavyJohnFeature.RecruitMapId;
            ClearMap = DavyJohnFeature.ClearMapId;
            MinMap = DavyJohnFeature.Stage1Map;
            MaxMap = DavyJohnFeature.Stage1Map + 500;

            EventTime = 4 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.setIntProperty(DavyJohnFeature.Stage2Property, 0);
            eim.setIntProperty(DavyJohnFeature.Stage2AProperty, 0);
            eim.setIntProperty(DavyJohnFeature.Stage3AProperty, 0);
            eim.setIntProperty("stage2b", 0);
            eim.setIntProperty("stage3b", 0);
            eim.setIntProperty("stage4", 0);
            eim.setIntProperty("stage5", 0);

            eim.setIntProperty("curStage", 1);
            eim.setIntProperty(DavyJohnFeature.GrindModeProperty, 0);

            eim.setIntProperty(DavyJohnFeature.OpenedChestsProperty, 0);
            eim.setIntProperty(DavyJohnFeature.OpenedBoxesProperty, 0);

            // 第2关 又分3个阶段，每个阶段只刷一种怪，所以不能用自带的刷新
            var map = await eim.getInstanceMap(DavyJohnFeature.Stage2Map);
            await map.killAllMonsters();

            // 风花房（过道）：每轮 4 只，5 轮
            await SpawnWindflowers(await eim.getInstanceMap(DavyJohnFeature.Stage3Map));
            await SpawnWindflowers(await eim.getInstanceMap(DavyJohnFeature.Stage4Map));

            await base.OnSetup(eim, level, lobbyId);
        }

        static async Task SpawnWindflowers(IMap? map)
        {
            if (map == null)
                return;

            for (var i = 0; i < 5; i++)
            {
                await map.spawnMonsterOnGroundBelow(DavyJohnFeature.WindflowerMobId1, 430, 75);
                await map.spawnMonsterOnGroundBelow(DavyJohnFeature.WindflowerMobId2, 1600, 75);
                await map.spawnMonsterOnGroundBelow(DavyJohnFeature.WindflowerMobId1, 430, 238);
                await map.spawnMonsterOnGroundBelow(DavyJohnFeature.WindflowerMobId2, 1600, 238);
            }
        }


        public override async Task respawnStages(AbstractEventInstanceManager eim)
        {
            // 第二阶段甲板：按已清理的轮次刷 9300114 + stage2。
            // 其余地图（含过道 202/302）的补充由 MapleMap.respawn() 按 life 的 mobTime 自动完成，无需手动调用。
            var stg = eim.getIntProperty(DavyJohnFeature.Stage2Property);
            if (stg < 3)
            {
                var map = await eim.getInstanceMap(DavyJohnFeature.Stage2Map);
                if (map != null)
                    await map.spawnAllMonsterIdFromMapSpawnList(9300114 + stg, eim.Level, true);
            }

            eim.Schedule(respawnStages, 10_000);
        }

        public override async Task OnPlayerMapChanged(AbstractEventInstanceManager eim, Player player, int mapid)
        {
            // 进入下一阶段时重置计时器：6/6/6/6/8 分钟
            var stage = eim.getIntProperty("curStage");
            var (expectedMap, nextStage, minutes) = stage switch
            {
                1 => (DavyJohnFeature.Stage2Map, 2, 6),
                2 => (DavyJohnFeature.Stage3Map, 3, 6),
                3 => (DavyJohnFeature.Stage4Map, 4, 6),
                4 => (DavyJohnFeature.Stage5Map, 5, 6),
                5 => (DavyJohnFeature.BossMap, 6, 8),
                _ => (0, 0, 0)
            };

            if (expectedMap != 0 && mapid == expectedMap)
            {
                await eim.restartEventTimer(minutes * 60_000);
                eim.setIntProperty("curStage", nextStage);
            }

            await base.OnPlayerMapChanged(eim, player, mapid);
        }

        public override async Task ClearPQ(AbstractEventInstanceManager eim)
        {
            await base.ClearPQ(eim);

            // 开箱数越多，通关经验越高
            var chests = eim.getIntProperty(DavyJohnFeature.OpenedChestsProperty);
            var expGain = chests == 0 ? 28000 : (chests == 1 ? 35000 : 42000);
            await eim.giveEventPlayersExp(expGain);

            await eim.warpEventTeam(DavyJohnFeature.ClearMapId);
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            var map = mob.getMap();
            var mapId = map.getId();

            // 过道房间：怪物自生自灭，不处理
            if (Array.IndexOf(PassageMaps, mapId) >= 0)
                return;

            if (Array.IndexOf(LordPirates, mob.getId()) >= 0)
            {
                await eim.dropMessage(5, "随着海盗领主死亡，无恙被释放了！");
                await eim.spawnNpc(2094001, new Point(777, 140), map);
            }

            if (map.countMonsters() == 0)
            {
                var stage = ((mapId % 1000) / 100) + 1;

                if ((stage == 1 || stage == 3 || stage == 4) && DavyJohnFeature.PassedGrindMode(map, eim))
                {
                    await eim.showClearEffect(mapId);
                }
                else if (stage == 5)
                {
                    var r1 = map.getReactorByName("sMob1")?.getState() ?? 0;
                    var r2 = map.getReactorByName("sMob2")?.getState() ?? 0;
                    var r3 = map.getReactorByName("sMob3")?.getState() ?? 0;
                    var r4 = map.getReactorByName("sMob4")?.getState() ?? 0;
                    if (r1 >= 1 && r2 >= 1 && r3 >= 1 && r4 >= 1)
                    {
                        await eim.showClearEffect(mapId);
                    }
                }
            }

            await base.OnMobKilled(eim, mob, killer);
        }
    }
}
