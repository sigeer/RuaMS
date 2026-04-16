using Application.Core.Channel;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.Scripting.Events;
using Application.Shared.Constants.Map;
using scripting.npc;
using tools;

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


        public override AbstractEventInstanceManager Setup(int level, int lobbyId)
        {
            return newInstance(Name + lobbyId.ToString());
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
            chr.sendPacket(PacketCreator.getClock(local / 1000));
            // chr.sendPacket(PacketCreator.earnTitleMessage("下一站停靠 " + (myRide == 0 ? "废都广场" : "废弃都市") + " 站。请走左侧门。"));

            eim.startEventTimer(local);
        }

        public override async Task HandleCreateInstanceResult(CreateInstanceResult r, NPCConversationManager cm)
        {
            switch (r)
            {
                case CreateInstanceResult.Success:
                    break;
                case CreateInstanceResult.LobbyLimited:
                    await cm.SayOK("客运车已经满了。稍后再试一次。");
                    break;
                case CreateInstanceResult.Disposed:
                case CreateInstanceResult.Unknown:
                    await cm.SayOK("未知错误");
                    break;
                default:
                    break;
            }
        }
    }
}
