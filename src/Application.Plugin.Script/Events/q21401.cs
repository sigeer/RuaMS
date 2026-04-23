using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    internal class q21401 : SoloEventManager
    {
        public q21401(WorldChannel cserv) : base(cserv, nameof(q21401))
        {
            MaxLobbys = 7;

            EventTime = 10 * 60;
            EntryMap = 914020000;
            ExitMap = 140000000;
            MinMap = 914020000;
            MaxMap = 914020000;
        }

        public override void OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() == 9001014)
            {
                eim.clearPQ();
            }
        }
    }
}
