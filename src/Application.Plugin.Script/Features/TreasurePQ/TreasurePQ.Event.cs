using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.TreasurePQ
{
    internal sealed class Event : AbstractPartyQuestEventTemplate
    {
        const int BossMobId = 9400589;

        public Event() : base(TreasurePQFeature.EventName)
        {
            MinCount = 4;
            MaxCount = 6;
            MinLevel = 140;
            MaxLevel = 255;

            RecruitMap = 674030100;
            EntryMap = 674030000;
            ExitMap = 674030100;
            ClearMap = 674030300;

            MinMap = 674030000;
            MaxMap = 674030300;

            EventTime = 45 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() != BossMobId)
                return;

            await eim.showClearEffect(mob.getMap().getId());
            await eim.GiveStageClearRewardAll(mob.getMap().getId());
            await eim.clearPQ();
        }
    }
}
