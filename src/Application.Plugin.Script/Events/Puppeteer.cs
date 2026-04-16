using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    internal class Puppeteer : SoloEventManager
    {
        public Puppeteer(WorldChannel cserv) : base(cserv, nameof(Puppeteer))
        {
            EventTime = 10 * 60;
            EntryMap = 910510000;
            ExitMap = 105070300;
            MinMap = 910510000;
            MaxMap = 910510000;
        }

        protected override void OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.getInstanceMap(910510000)?.resetFully();
            eim.getInstanceMap(910510000)?.allowSummonState(false);
        }
    }
}
