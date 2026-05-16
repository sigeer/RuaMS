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

            MapObjectType.MINI_GAME,
            MapObjectType.PLAYER_NPC,
            MapObjectType.HIRED_MERCHANT
        ];

        // rangedMapobjectTypes 和 NonRangedType都包含了NPC
        public static bool isNonRangedType(MapObjectType type)
        {
            switch (type)
            {
                // rangedMapobjectTypes包含了NPC
                case MapObjectType.NPC:
                // rangedMapobjectTypes包含了PLAYER_SHOP，性质应该和HIRED_MERCHANT一样？
                case MapObjectType.HIRED_MERCHANT:
                // PlayerNPC应该同NPC？
                case MapObjectType.PLAYER_NPC:

                case MapObjectType.PLAYER:
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

        public static bool IsObjectInRange(IMapObject obj, Point source, double rangeSq)
        {
            return source.distanceSq(obj.getPosition()) <= rangeSq;
        }

    }
}
