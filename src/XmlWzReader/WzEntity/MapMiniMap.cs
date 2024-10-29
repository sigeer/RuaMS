using System.Drawing;

namespace XmlWzReader.WzEntity
{
    public class MapMiniMap
    {
        public MapMiniMap(int mapId, Data minimapData)
        {
            MapId = mapId;

            CenterX = DataTool.getInt(minimapData.getChildByPath("centerX")) * -1;
            CenterY = DataTool.getInt(minimapData.getChildByPath("centerY")) * -1;
            Height = DataTool.getInt(minimapData.getChildByPath("height"));
            Width = DataTool.getInt(minimapData.getChildByPath("width"));
        }

        public int Id { get; set; }
        public int MapId { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
        public int CenterX { get; set; }
        public int CenterY { get; set; }
        public int Mag { get; set; }
    }
}
