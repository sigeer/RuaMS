using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.PapulatusBattle
{
    internal sealed class Event : AbstractPartyQuestEventTemplate
    {
        public Event() : base(PapulatusBattleFeature.EventName)
        {
            MinCount = 1;
            MaxCount = 6;
            MinLevel = 1;
            MaxLevel = 255;

            RecruitMap = 220080000;
            EntryMap = 220080001;
            ExitMap = 220080000;
            ClearMap = 220080000;

            MinMap = 220080001;
            MaxMap = 220080001;

            EventTime = 45 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }


        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() != 8500002)
                return;

            await eim.showClearEffect(mob.getMap().getId());
            await eim.clearPQ();
        }


    }
}
