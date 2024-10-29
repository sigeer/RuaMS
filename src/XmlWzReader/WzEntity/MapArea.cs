using System.Drawing;

namespace XmlWzReader.WzEntity
{
    public class MapArea
    {
        public MapArea(int mapId, Data areaData)
        {
            MapId = mapId;

            X1 = DataTool.getInt(areaData.getChildByPath("x1"));
            Y1 = DataTool.getInt(areaData.getChildByPath("y1"));
            X2 = DataTool.getInt(areaData.getChildByPath("x2"));
            Y2 = DataTool.getInt(areaData.getChildByPath("y2"));
        }

        public int Id { get; set; }
        public int MapId { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }

        public Rectangle Rect => new Rectangle(X1, Y1, X2 - X1, Y2 - Y1);
    }
}
