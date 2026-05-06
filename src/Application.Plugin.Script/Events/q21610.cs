using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    internal class q21610 : SoloEventManager
    {
        public q21610(WorldChannel cserv) : base(cserv, nameof(q21610))
        {
            EventTime = 3 * 60;
            EntryMap = 921110000;
            ExitMap = 211050000;
            MinMap = 921110000;
            MaxMap = 921110000;
        }

        public override void OnFriendlyMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            End(eim);
        }
    }
}
