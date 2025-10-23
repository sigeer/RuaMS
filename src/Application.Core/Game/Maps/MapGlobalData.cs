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
        public static Dictionary<int, KeyValuePair<int, int>?> dropBoundsCache = new(100);
    }
}
