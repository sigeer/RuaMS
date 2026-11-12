using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.YaoSengPQ
{
    internal sealed class Event : AbstractPartyQuestEventTemplate
    {
        const int BossMobId = 9600025;

        /// <summary>Boss 掉落：物品、最大数量、掉率。</summary>
        const int DropItemId = 2000005;
        const int DropMaxCount = 5;
        const double DropChance = 0.4;

        public Event() : base(YaoSengPQFeature.EventName)
        {
            MinCount = 1;
            MaxCount = 6;
            MinLevel = 120;
            MaxLevel = 255;

            RecruitMap = 702070400;
            EntryMap = 702060000;
            ExitMap = 702070400;
            ClearMap = 702060000;

            MinMap = 702060000;
            MaxMap = 702060000;

            EventTime = 30 * 60;
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            // JS 只在击杀 Boss 时通关；打小怪不应触发
            if (eim.isEventCleared() || mob.getId() != BossMobId)
                return;

            var map = mob.getMap();
            var dropper = eim.getPlayers().FirstOrDefault();
            if (dropper != null)
            {
                // 0~5 个，每个 40%
                for (var i = 0; i < DropMaxCount; i++)
                {
                    if (Random.Shared.NextDouble() >= DropChance)
                        continue;

                    var item = Application.Core.Channel.DataProviders.ItemInformationProvider.getInstance()
                        .GenerateVirtualItemById(DropItemId, 1);
                    await map.spawnItemDrop(mob, dropper, item, mob.getPosition(), true, false);
                }
            }

            await eim.clearPQ();
        }

        /// <summary>JS 的 allMonstersDead：场上怪物全清时也直接通关。</summary>
        public override async Task OnMobClear(AbstractEventInstanceManager eim, IMap map)
        {
            if (!eim.isEventCleared())
                await eim.clearPQ();

            await base.OnMobClear(eim, map);
        }

        public override async Task ClearPQ(AbstractEventInstanceManager eim)
        {
            await base.ClearPQ(eim);

            // 通关后再给 2 分钟清场时间
            await eim.startEventTimer(2 * 60_000);
        }

    }
}
