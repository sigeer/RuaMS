using Application.Shared.Constants.Map;

namespace Application.Plugin.TreasureMap
{
    internal class Settings
    {
        /// <summary>
        /// 仅在此频道中开放
        /// </summary>
        public const int ActiveChannel = 2;
        /// <summary>
        /// 藏宝图ItemId
        /// </summary>
        public const int TreasureMapItemId = 2430030;
        /// <summary>
        /// 领取任务消耗
        /// </summary>
        public const int QuestPrice = 10000;

        /// <summary>
        /// 藏宝图地图
        /// </summary>
        public static int[] Maps = [
            MapId.HENESYS, MapId.HENESYS_PARK,
            MapId.KERNING_CITY,
            MapId.PERION,
            MapId.ELLINIA,
            MapId.LITH_HARBOUR,
            MapId.SLEEPYWOOD,
            ];

        /// <summary>
        /// 怪物所在地图
        /// </summary>
        public static int[] MobMaps = [
            100010000,
            100020000,

            100050000,
            101010000,

            101030000,
            102010000,

            102050000,
            103010000,
            ];

        /// <summary>
        /// level.45
        /// </summary>
        public static int[] Mobs = [9400100, 9400101, 9400102, 9400103];


        public const double NEAR_THRESHOLD = 10000;   // 小于此值为“很近”
    }
}
