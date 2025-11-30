using Prometheus;

namespace Application.Core.Channel.Performance
{
    public class GameMetrics
    {
        readonly WorldChannel _worldChannel;

        string channelLabel;
        public GameMetrics(WorldChannel channel)
        {
            _worldChannel = channel;

            channelLabel = _worldChannel.Id.ToString();

            var gauageConfig = new GaugeConfiguration
            {
                LabelNames = new[] { $"channelId" }
            };
            _channelPlayers = Metrics.CreateGauge(
            "game_online_players",
            "在线玩家数", gauageConfig);

            _activeMaps = Metrics.CreateGauge(
            "game_active_maps",
            "活跃地图数", gauageConfig);

            _mapPlayerCount = Metrics.CreateGauge(
            "game_map_player_count",
            "地图玩家数量",
            new GaugeConfiguration
            {
                LabelNames = new[] { $"channelId", "mapId" }
            });

            _eventInstanceCount = Metrics.CreateGauge(
            "game_active_eventinstance",
            "正在游戏副本数量", gauageConfig);

            _mapTickDuration = Metrics.CreateHistogram(
            "game_map_tick_duration_ms",
            "地图Tick耗时（毫秒）",
            new HistogramConfiguration
            {
                LabelNames = new[] { $"channelId" },
                Buckets = Histogram.LinearBuckets(start: 5, width: 5, count: 20) // 5ms~100ms
            });
        }

        Gauge _channelPlayers;
        /// <summary>
        /// 在线玩家数
        /// </summary>
        public Gauge.Child ChannelPlayers => _channelPlayers.WithLabels(channelLabel);

        Gauge _eventInstanceCount;
        /// <summary>
        /// 正在游戏副本数量
        /// </summary>
        public Gauge.Child EventInstanceCount => _eventInstanceCount.WithLabels(channelLabel);

        /// <summary>
        /// 活跃地图数
        /// </summary>
        Gauge _activeMaps;
        public Gauge.Child ActiveMaps => _activeMaps.WithLabels(channelLabel);


        /// <summary>
        /// 地图玩家数量
        /// </summary>
        Gauge _mapPlayerCount;
        public void MapCountInc(int mapId)
        {
            _mapPlayerCount.WithLabels(channelLabel, mapId.ToString()).Inc();
        }
        public void MapCountDec(int mapId)
        {
            _mapPlayerCount.WithLabels(channelLabel, mapId.ToString()).Dec();
        }

        /// <summary>
        /// 地图Tick耗时（毫秒）
        /// </summary>
        Histogram _mapTickDuration;
        // 延迟/耗时分布
        public void MapTick(double dur)
        {
            _mapTickDuration.WithLabels(channelLabel).Observe(dur);
        }
    }
}
