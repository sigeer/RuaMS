using Application.Core.Client;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.PrivateContiMove
{
    /// <summary>
    /// 航线基类：站点、途中地图与行驶时长由子类指定。
    /// 各航线（克宁号、天空之城各线）分别定义在同目录的独立文件中。
    /// </summary>
    internal abstract class PrivateContiMoveBase : AbstractSoloEventTemplate
    {
        protected PrivateContiMoveBase(string name, int[] stations, int[] transportings, int rideTime)
            : base(name)
        {
            Stations = stations;
            Transportings = transportings;
            RideTime = rideTime;
            ArrivePortals = [0, 0];
            MaxLobbys = 16;
        }

        public int[] Stations { get; }
        public int[] Transportings { get; }
        public int[] ArrivePortals { get; init; }
        public int RideTime { get; }

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

            eim.setIntProperty("Current", curIdx);
            await chr.changeMap(Transportings[curIdx], 0);
        }

        public override async Task OnPlayerMapChanging(AbstractEventInstanceManager eim, Player player, int mapid)
        {
            if (!Transportings.Contains(mapid))
            {
                await End(eim, TerminationReason.Failure);
            }
        }

        public override async Task OnTimeOut(AbstractEventInstanceManager eim)
        {
            var curIdx = eim.getIntProperty("Current");
            var map = await eim.getMapInstance(Transportings[curIdx]);
            var nextIdx = 1 - curIdx;
            await map.warpEveryone(Stations[nextIdx], ArrivePortals[nextIdx]);
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
