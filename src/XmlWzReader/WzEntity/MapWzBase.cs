using System.Drawing;
using System.Net.Http.Headers;

namespace XmlWzReader.WzEntity
{
    public class MapWzBase
    {
        public static bool IsCPQMap(int mapId)
        {
            switch (mapId)
            {
                case 980000101:
                case 980000201:
                case 980000301:
                case 980000401:
                case 980000501:
                case 980000601:

                case 980031100:
                case 980032100:
                case 980033100:
                    return true;
            }
            return false;
        }
        public MapWzBase(int id, Data mapData)
        {
            Id = id;
            Clock = mapData.getChildByPath("clock") != null;
            Boat = mapData.getChildByPath("shipObj") != null;
            SeatCount = mapData.getChildByPath("seat")?.getChildren()?.Count ?? 0;

            var infoData = mapData.getChildByPath("info")!;
            MapInfo = new MapInfo(Id, infoData);

            Portals = [];
            var portalData = mapData.getChildByPath("portal");
            if (portalData != null)
                Portals = portalData.Select(x => new MapPortalWz(Id, x)).ToList();

            Areas = [];
            var areaData = mapData.getChildByPath("area");
            if (areaData != null)
                Areas = areaData.Select(x => new MapArea(Id, x)).ToList();

            Reactors = [];
            var reactorData = mapData.getChildByPath("reactor");
            if (reactorData != null)
                Reactors = reactorData.Select(x => new MapReactor(Id, x)).Where(x => x.IsValid).ToList();

            Lives = [];
            var lifeData = mapData.getChildByPath("life");
            if (lifeData != null)
                Lives = lifeData.Select(x => new MapLife(Id, x)).ToList();

            var cpqInfo = mapData.getChildByPath("monsterCarnival");
            if (IsCPQMap(Id) && cpqInfo != null)
                CPQInfo = new MapMonsterCarnivalInfo(Id, cpqInfo);

            Backs = [];
            var backData = mapData.getChildByPath("back");
            if (backData != null)
                Backs = backData.Select(x => new MapBack(Id, x)).ToList();

            if (MapInfo.VRTop == MapInfo.VRBottom)
            {
                // old-style baked map
                var minimapData = mapData.getChildByPath("miniMap");
                if (minimapData != null)
                {
                    MiniMapInfo = new MapMiniMap(Id, minimapData);
                    MapArea = new Rectangle(MiniMapInfo.CenterX, MiniMapInfo.CenterY, MiniMapInfo.Width, MiniMapInfo.Height);
                }
                else
                {
                    int dist = (1 << 18);
                    MapArea = new Rectangle(-dist / 2, -dist / 2, dist, dist);
                }
            }
            else
            {
                MapArea = new Rectangle(MapInfo.VRLeft, MapInfo.VRTop, MapInfo.VRRight - MapInfo.VRLeft, MapInfo.VRBottom - MapInfo.VRTop);
            }

            Footholds = [];
            var footholdData = mapData.getChildByPath("foothold")?.SelectMany(x => x.getChildren())?.SelectMany(x => x.getChildren());
            if (footholdData != null)
                Footholds = footholdData.Select(x => new MapFoothold(Id, x)).ToList();

        }
        public int Id { get; set; }

        //public string Name { get; set; }
        //public string StreetName { get; set; }

        public bool Clock { get; set; }
        public bool Boat { get; private set; }
        public int SeatCount { get; set; }
        public MapInfo MapInfo { get; set; }
        public MapMiniMap? MiniMapInfo { get; set; }
        public List<MapBack> Backs { get; set; }
        public List<MapLife> Lives { get; set; }
        public List<MapPortalWz> Portals { get; set; }
        public List<MapArea> Areas { get; set; }
        public List<MapReactor> Reactors { get; set; }
        public List<MapFoothold> Footholds { get; set; }
        public MapMonsterCarnivalInfo? CPQInfo { get; set; }
        public Rectangle MapArea { get; set; }

        public int FootholdMinX1 => Footholds.Min(x => x.X1);
        public int FootholdMinY1 => Footholds.Min(x => x.Y1);
        public int FootholdMaxX2 => Footholds.Max(x => x.X2);
        public int FootholdMaxY2 => Footholds.Max(x => x.Y2);
    }
}
