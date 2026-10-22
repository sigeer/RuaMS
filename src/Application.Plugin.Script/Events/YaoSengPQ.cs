using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    /// <summary>
    /// 妖僧组队任务。
    /// 对应 JS scripts/event/YaoSengPQ.js。
    /// </summary>
    internal class YaoSengPQ : AbstractPartyQuestEventTemplate
    {
        const int BossMobId = 9600025;

        public YaoSengPQ() : base(nameof(YaoSengPQ))
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
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (eim.isEventCleared())
                return;

            if (mob.getId() == BossMobId)
            {
                var dropper = eim.getPlayers().FirstOrDefault();
                if (dropper != null)
                {
                    var item = Application.Core.Channel.DataProviders.ItemInformationProvider.getInstance().GenerateVirtualItemById(2000005, 1);
                    await mob.getMap().spawnItemDrop(mob, dropper, item, mob.getPosition(), true, false);
                }
            }

            await eim.clearPQ();
        }
    }
}
