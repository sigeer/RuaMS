using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    internal class RockSpirit : SoloEventManager
    {
        public RockSpirit(WorldChannel cserv) : base(cserv, nameof(RockSpirit))
        {
            EventTime = 60 * 60;
            EntryMap = 103040410;
            ExitMap = 103040400;
            MinMap = 103040410;
            MaxMap = 103040410;
        }

        protected override void respawnStages(AbstractEventInstanceManager eim)
        {
            var map = eim.getMapInstance(EntryMap);
            var map2 = eim.getMapInstance(103040420);
            map.allowSummonState(true);
            map2.allowSummonState(true);
            map.instanceMapRespawn();
            map2.instanceMapRespawn();

            eim.Schedule(respawnStages, 10000);
        }
    }
}
