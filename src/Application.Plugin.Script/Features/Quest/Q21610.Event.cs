using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21610Event : SoloQuestEventTemplate
    {
        public Q21610Event() : base(Q21610Feature.QuestIdValue)
        {
            EventTime = 3 * 60;
            EntryMap = 921110000;
            EntryPortal = 2;

            ExitMap = 211050000;
            MinMap = 921110000;
            MaxMap = 921110000;
        }

        public override async Task OnFriendlyMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            await End(eim, Core.scripting.Events.Abstraction.TerminationReason.Failure);
        }

    }
}
