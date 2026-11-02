using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.LatanicaBattle
{
    internal sealed class Event : AbstractPartyQuestEventTemplate
    {
        public Event() : base(LatanicaBattleFeature.EventName)
        {
            MinCount = 1;
            MaxCount = 6;
            MinLevel = 1;
            MaxLevel = 255;

            RecruitMap = 541010060;
            EntryMap = 541010100;
            ExitMap = 541010110;
            ClearMap = 541010110;

            MinMap = 541010100;
            MaxMap = 541010100;

            EventTime = 10 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() != 9420513)
                return;

            await eim.showClearEffect(mob.getMap().getId());
            await eim.clearPQ();
        }


    }
}
