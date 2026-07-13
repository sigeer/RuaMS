using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    internal class q21610 : AbstractSoloEventTemplate
    {
        public q21610() : base(nameof(q21610))
        {
            EventTime = 3 * 60;
            EntryMap = 921110000;
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
