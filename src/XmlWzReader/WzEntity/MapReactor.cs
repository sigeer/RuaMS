using System.Drawing;

namespace XmlWzReader.WzEntity
{
    public class MapReactor
    {
        public MapReactor(int mapId, Data mapReactor)
        {
            MapId = mapId;

            ReactorId = DataTool.getIntConvert(mapReactor.getChildByPath("id"));
            X = DataTool.getInt(mapReactor.getChildByPath("x"));
            Y = DataTool.getInt(mapReactor.getChildByPath("y"));
            FacingDirection = (sbyte)DataTool.getInt(mapReactor.getChildByPath("f"));
            ReactorTime = DataTool.getInt(mapReactor.getChildByPath("reactorTime")) * 1000;
            Name = DataTool.getString(mapReactor.getChildByPath("name")) ?? "";
        }

        public int Id { get; set; }
        public int MapId { get; set; }

        public int ReactorId { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public Point Point => new Point(X, Y);
        public int ReactorTime { get; set; }
        public string Name { get; set; }
        public sbyte FacingDirection { get; set; }

        public bool IsValid => ReactorId > 0;
    }
}
