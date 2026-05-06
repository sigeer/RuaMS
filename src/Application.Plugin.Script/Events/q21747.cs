using Application.Core.Channel;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Shared.Constants.Npc;
using System.Drawing;

namespace Application.Plugin.Script.Events
{
    internal class q21747 : SoloEventManager
    {
        public q21747(WorldChannel cserv) : base(cserv, nameof(q21747))
        {
            EventTime = 10 * 60;
            EntryMap = 925040100;
            ExitMap = 250020300;
            MinMap = 925040100;
            MaxMap = 925040100;
        }

        protected override void OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            base.OnSetup(eim, level, lobbyId);
            eim.getMapInstance(925040100).SpawnNpc(1204020, new Point(850, 0));
        }
    }
}
