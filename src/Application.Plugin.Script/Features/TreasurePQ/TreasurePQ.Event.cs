using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.TreasurePQ
{
    /// <summary>
    /// 寻宝组队任务。对应 JS <c>scripts/event/TreasurePQ.js</c>。
    /// 击杀 MV（9400589）后通关，10 秒后传送到宝库 674030300，限时 10 分钟。
    /// </summary>
    internal sealed class Event : AbstractPartyQuestEventTemplate
    {
        const int BossMobId = 9400589;

        /// <summary>宝库地图。</summary>
        const int BonusMap = 674030300;

        /// <summary>通关后进入宝库的奖励时间（分钟）。</summary>
        const int BonusTime = 10;

        public Event() : base(TreasurePQFeature.EventName)
        {
            MinCount = 4;
            MaxCount = 6;
            MinLevel = 140;
            MaxLevel = 255;

            RecruitMap = 674030100;
            EntryMap = 674030000;
            ExitMap = 674030100;
            ClearMap = BonusMap;

            MinMap = 674030000;
            MaxMap = BonusMap;

            ExcludedMap = [ExitMap];

            EventTime = 45 * 60;

            // JS expStages = [60000, 100000]，只用到第 2 关
            StageClearRewards = new()
            {
                { 2, new(100_000, 0) },
            };
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.setProperty("statusStg1", "0");

            // 674030000 的 Map.wz 没有 reactorShuffle 节点，需要事件侧洗牌
            (await eim.getInstanceMap(674030000))?.shuffleReactors();

            await base.OnSetup(eim, level, lobbyId);
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() == BossMobId)
            {
                await eim.showClearEffect(mob.getMap().getId());
                await eim.GiveStageClearRewardAll(2);
                await eim.clearPQ();
            }

            await base.OnMobKilled(eim, mob, killer);
        }

        public override async Task ClearPQ(AbstractEventInstanceManager eim)
        {
            await base.ClearPQ(eim);

            if (eim.GetStageStatus(BonusMap) == StageStatus.NotStarted)
            {
                eim.StartStage(BonusMap);
                // 10 秒后进入宝库，并开始 10 分钟奖励计时
                eim.Schedule(async instance =>
                {
                    await instance.startEventTimer(BonusTime * 60_000);
                    await instance.warpEventTeam(BonusMap);
                }, 10_000);
            }
        }
    }
}
