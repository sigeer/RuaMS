using System.Collections.Concurrent;

namespace Application.Core.Game.Maps
{
    public class MapGlobalData
    {
        public static List<MapObjectType> rangedMapobjectTypes = [
            MapObjectType.PLAYER_SHOP,
            MapObjectType.ITEM,
            MapObjectType.NPC,
            MapObjectType.MONSTER,
            MapObjectType.DOOR,
            MapObjectType.SUMMON,
            MapObjectType.REACTOR];
        public static ConcurrentDictionary<int, KeyValuePair<int, int>?> dropBoundsCache = new(-1, 100);
    }
}
