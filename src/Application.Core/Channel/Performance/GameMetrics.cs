using Application.Core.Game.Maps;
using OpenTelemetry.Metrics;
using System.Diagnostics.Metrics;

namespace Application.Core.Channel.Performance
{
    public class GameMetrics
    {
        private static readonly Meter MyMeter = new("RuaMS.GameMetrics", "1.0.0");

        public static void RegisterChannel(WorldChannelServer server)
        {
            MyMeter.CreateObservableGauge<int>(
                "game_online_players",
                () => CollectChannelPlayers(server),
                description: "在线玩家数");

            MyMeter.CreateObservableGauge<int>(
                "game_active_maps",
                () => CollectChannelMaps(server),
                description: "活跃地图数");

            MyMeter.CreateObservableGauge<int>(
                "game_map_player_count",
                () => CollectMapPlayers(server),
                description: "地图玩家数量");


            MyMeter.CreateObservableGauge<int>(
                "game_active_eventinstance",
                () => CollectEventInstance(server),
                description: "正在游戏副本数量");

        }

        static IEnumerable<Measurement<int>> CollectChannelPlayers(WorldChannelServer server)
        {
            foreach (var worldChannel in server.Servers.Values)
            {
                yield return new Measurement<int>(
                worldChannel.Players.Count(),
                new KeyValuePair<string, object?>("channelId", worldChannel.Id.ToString()));
            }
        }

        static IEnumerable<Measurement<int>> CollectChannelMaps(WorldChannelServer server)
        {
            foreach (var worldChannel in server.Servers.Values)
            {
                yield return new Measurement<int>(
                        worldChannel.GetActiveMapCount(),
                        new KeyValuePair<string, object?>("channelId", worldChannel.Id.ToString())
                    );
            }
        }

        static IEnumerable<Measurement<int>> CollectMapPlayers(WorldChannelServer server)
        {
            var dataSource = server.PlayerStorage.getAllCharacters().Where(x => x.isLoggedinWorld()).GroupBy(x => x.Channel)
                .ToDictionary(x => x.Key, x => x.GroupBy(y => y.MapModel).Select(y => new { map = y.Key, count = y.Count() }));
            foreach (var ch in dataSource)
            {

                foreach (var item in ch.Value)
                {
                    yield return new Measurement<int>(
                        item.count,
                        new KeyValuePair<string, object?>("channelId", ch.ToString()),
                        new KeyValuePair<string, object?>("mapName", item.map.InstanceName)
                    );
                }

            }
        }

        static IEnumerable<Measurement<int>> CollectEventInstance(WorldChannelServer server)
        {
            foreach (var worldChannel in server.Servers.Values)
            {
                yield return new Measurement<int>(
                        worldChannel.getEventSM().GetEventInstanceCount(),
                        new KeyValuePair<string, object?>("channelId", worldChannel.Id.ToString())
                    );
            }
        }

        public void RegisterMap(IMap map)
        {
            var p = MyMeter.CreateObservableGauge<int>(
            "game_map_player_count",
            () =>
            {
                return new Measurement<int>(
                    map.getAllPlayers().Count,
                    new KeyValuePair<string, object?>("channelId", map.ChannelServer.Id.ToString()),
                    new KeyValuePair<string, object?>("mapName", map.InstanceName)
                );
            },
            description: "地图玩家数量");
        }

        /// <summary>
        /// 地图Tick耗时（毫秒）
        /// </summary>
        static Histogram<double> _mapTickDuration = MyMeter.CreateHistogram<double>(
            name: "game_map_tick_duration_ms",
            unit: "ms",
            description: "地图Tick耗时（毫秒）");
        public static void MapTick(double dur, WorldChannel worldChannel)
        {
            _mapTickDuration.Record(dur, new KeyValuePair<string, object?>("channelId", worldChannel.Id.ToString()));
        }
    }
}
