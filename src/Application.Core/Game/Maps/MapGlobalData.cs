using System.Collections.Concurrent;

namespace Application.Core.Game.Maps
{
    public class MapGlobalData
    {
        public static HashSet<MapObjectType> rangedMapobjectTypes = [
            MapObjectType.PLAYER_SHOP,
            MapObjectType.ITEM,
            MapObjectType.NPC,
            MapObjectType.MONSTER,
            MapObjectType.DOOR,
            MapObjectType.SUMMON,
            MapObjectType.REACTOR,
            MapObjectType.MINI_GAME
        ];

        public static HashSet<MapObjectType> GetRangedMapObjectTypes() => YamlConfig.config.server.USE_MAXRANGE ? [] : rangedMapobjectTypes;

        // rangedMapobjectTypes 和 NonRangedType都包含了NPC
        public static bool isNonRangedType(MapObjectType type)
        {
            switch (type)
            {
                // case MapObjectType.NPC:
                case MapObjectType.PLAYER:
                case MapObjectType.HIRED_MERCHANT:
                case MapObjectType.PLAYER_NPC:
                case MapObjectType.DRAGON:
                case MapObjectType.MIST:
                case MapObjectType.KITE:
                    return true;
                default:
                    return false;
            }
        }

        public static ConcurrentDictionary<int, KeyValuePair<int, int>?> dropBoundsCache = new(-1, 100);

        public static double getRangedDistance()
        {
            return YamlConfig.config.server.USE_MAXRANGE ? double.PositiveInfinity : RangedDistance;
        }

        public const double RangedDistance = 722500;
    }
}
