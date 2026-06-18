using Application.Core.Client;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    public class PrivateContiMove : AbstractSoloEventTemplate
    {
        public int[] Stations { get; }
        public int[] Transportings { get; }
        public int[] ArrivePortals { get; init; }
        public int RideTime { get; }

        public PrivateContiMove(string name, int[] stations, int[] transportings, int rideTime) : base(name)
        {
            Stations = stations;
            Transportings = transportings;
            RideTime = rideTime;
            ArrivePortals = [0, 0];
            MaxLobbys = 16;
        }

        public override Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            EventTime = eim.ChannelServer.getTransportationTime(RideTime);
            return Task.CompletedTask;
        }

        public override async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            int curIdx = Stations.IndexOf(chr.getMapId());
            if (curIdx == -1)
            {
                return;
            }

            eim.Properties["Current"] = curIdx.ToString();
            await chr.changeMap(Transportings[curIdx], 0);
            // chr.sendPacket(PacketCreator.earnTitleMessage("下一站停靠 " + (myRide == 0 ? "废都广场" : "废弃都市") + " 站。请走左侧门。"));
        }

        public override async Task OnPlayerMapChanging(AbstractEventInstanceManager eim, Player player, int mapid)
        {
            if (!Transportings.Contains(mapid))
            {
                await End(eim);
            }
        }

        public override async Task OnTimeOut(AbstractEventInstanceManager eim)
        {
            if (int.TryParse(eim.Properties.GetValueOrDefault("Current"), out var curIdx))
            {
                var map =  await eim.getMapInstance(Transportings[curIdx]);
                await map.warpEveryone(Stations[1 - curIdx], 0);
            }
        }

        public override string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case CreateInstanceResult.Success:
                    return null;
                case CreateInstanceResult.LobbyLimited:
                    return "已经满员了。稍后再试一次。";
                case CreateInstanceResult.Disposed:
                case CreateInstanceResult.Unknown:
                default:
                    return "未知错误";
            }
        }
    }
}
