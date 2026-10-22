using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    internal class q21613 : AbstractSoloEventTemplate
    {
        public q21613() : base(nameof(q21613))
        {
            EventTime = 3 * 60;
            EntryMap = 914030000;
            ExitMap = 140010210;
            MinMap = 914030000;
            MaxMap = 914030000;
        }

        public override async Task OnMobClear(AbstractEventInstanceManager eim, IMap map)
        {
            await eim.showClearEffect();
        }

        public override async Task OnFriendlyMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            await End(eim, Core.scripting.Events.Abstraction.TerminationReason.Failure);
        }
    }
}
