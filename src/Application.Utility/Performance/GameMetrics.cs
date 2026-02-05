using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Application.Utility.Performance
{
    public class GameMetrics
    {
        public const string MetricsName = "RuaMS.GameMetrics";
        public const string ActivityName = "RuaMS.GameServer";

        public static ActivitySource ActivitySource { get; set; } = new ActivitySource(ActivityName, "1.0.0");

        private static readonly Meter MyMeter = new(MetricsName, "1.0.0");

        public static readonly UpDownCounter<int> CasgShopPlayerCount = MyMeter.CreateUpDownCounter<int>("game_cashshop_players", description: "商城玩家数");
        public static readonly UpDownCounter<int> OnlinePlayerCount = MyMeter.CreateUpDownCounter<int>("game_online_players", description: "在线玩家数");
        public static readonly UpDownCounter<int> ActiveMapCount = MyMeter.CreateUpDownCounter<int>("game_active_maps", description: "活跃地图数");
        public static readonly UpDownCounter<int> MapPlayerCount = MyMeter.CreateUpDownCounter<int>("game_map_player_count", description: "地图玩家数量");
        public static readonly UpDownCounter<int> ChannelEventInstanceCount = MyMeter.CreateUpDownCounter<int>("game_active_eventinstance", description: "频道事件副本数量");

        /// <summary>
        /// 地图Tick耗时（毫秒）
        /// </summary>
        public static Histogram<double> MapTickDuration = MyMeter.CreateHistogram<double>(
            name: "game_map_tick_duration_ms",
            unit: "ms",
            description: "地图重生耗时（毫秒）");

        /// <summary>
        /// 执行命令耗时
        /// </summary>
        static Histogram<double> _worldTickDuration = MyMeter.CreateHistogram<double>(
            name: "game_world_tick_duration",
            unit: "ms",
            description: "命令执行耗时（毫秒）");
        public static void GameTick(string instance, double dur)
        {
            _worldTickDuration.Record(dur, new KeyValuePair<string, object?>("Scope", instance));
        }

        /// <summary>
        /// 命令数量
        /// </summary>
        static Histogram<int> _commandCount = MyMeter.CreateHistogram<int>(
            name: "game_world_tick_count",
            description: "单次命令数量");
        public static void CommandCountTick(string instance, int count)
        {
            _commandCount.Record(count, new KeyValuePair<string, object?>("Scope", instance));
        }
    }
}
