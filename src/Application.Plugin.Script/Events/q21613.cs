using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    internal class q21613 : SoloEventManager
    {
        public q21613(WorldChannel cserv) : base(cserv, nameof(q21613))
        {
            EventTime = 3 * 60;
            EntryMap = 914030000;
            ExitMap = 140010210;
            MinMap = 914030000;
            MaxMap = 914030000;
        }

        public override void OnMobClear(AbstractEventInstanceManager eim, IMap map)
        {
            eim.showClearEffect();
        }

        public override void OnFriendlyMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            End(eim);
        }
    }
}
