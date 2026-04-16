using Application.Core.Channel;
using Application.Core.Client;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    public class PrivateContiMove : SoloEventManager
    {
        public PrivateContiMove(WorldChannel cserv, string name,
            int[] stations, int[] transportings, int rideTime) : base(cserv, name)
        {
            Stations = stations;
            Transportings = transportings;
            RideTime = rideTime;
        }

        public int[] Stations { get; }

        public int[] Transportings { get; }

        public int RideTime { get; }

        public int GetTransportingTime()
        {
            return ChannelServer.getTransportationTime(RideTime);
        }


        protected override void OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            EventTime = GetTransportingTime();
        }

        public override void OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            int curIdx = Stations.IndexOf(chr.getMapId());
            if (curIdx == -1)
            {
                return;
            }

            var local = GetTransportingTime();
            var onRide = ChannelServer.getMapFactory().getMap(Transportings[1 - curIdx]);

            chr.changeMap(onRide, onRide.getPortal(0));
            // chr.sendPacket(PacketCreator.earnTitleMessage("下一站停靠 " + (myRide == 0 ? "废都广场" : "废弃都市") + " 站。请走左侧门。"));
        }

        public override string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case CreateInstanceResult.Success:
                    return null;
                case CreateInstanceResult.LobbyLimited:
                    return "客运车已经满了。稍后再试一次。";
                case CreateInstanceResult.Disposed:
                case CreateInstanceResult.Unknown:
                default:
                    return "未知错误";
            }
        }
    }
}
