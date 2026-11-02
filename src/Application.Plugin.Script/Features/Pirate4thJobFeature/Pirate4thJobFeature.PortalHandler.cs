using Application.Core.Client;
using Application.Core.scripting.Events.Instances;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Pirate4thJobFeature
{
    internal sealed class Pirate4thJobFeaturePortalHandler : PortalPlayerInteraction
    {
        public Pirate4thJobFeaturePortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        AbstractEventInstanceManager? GetEventInstance(string evtName)
        {
            var eim = getEventInstance();
            return eim != null && eim.EventName == evtName ? eim : null;
        }

        // Map: 912010100 | pt: 7
        public Task<bool> s4ship_out() => LeaveByCanLeave(Pirate4thJobFeature.ShipEventName);

        // Map: 912010000 | pt: 7
        public Task<bool> s4super_out() => LeaveByCanLeave(Pirate4thJobFeature.SuperEventName);

        async Task<bool> LeaveByCanLeave(string evtName)
        {
            var eim = GetEventInstance(evtName);
            if (eim == null)
            {
                return false;
            }

            var exit = eim.getIntProperty("canLeave");
            if (exit == 0)
            {
                await Pink("You have to wait one minute before you can leave this place.");
                return false;
            }

            await playPortalSound();
            await warp(exit == 2 ? 912010200 : 120000101);
            return true;
        }

        // Map: 925010300 | pt: 7
        public async Task<bool> s4mind_end()
        {
            var eim = GetEventInstance(Pirate4thJobFeature.MindEventName);
            if (eim == null || !eim.isEventCleared())
            {
                await Pink("You have to clear this mission before entering this portal.");
                return false;
            }

            if (isQuestStarted(Pirate4thJobFeatureEvent6410.QuestIdValue))
            {
                await setQuestProgress(Pirate4thJobFeatureEvent6410.QuestIdValue, 6411, "p2");
            }

            await playPortalSound();
            await warp(925010400);
            return true;
        }
    }
}
