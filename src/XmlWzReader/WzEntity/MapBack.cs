namespace XmlWzReader.WzEntity
{
    public class MapBack
    {
        public MapBack(int mapId, Data backData)
        {
            MapId = mapId;

            Index = int.Parse(backData.getName()!);
            Type = DataTool.getInt(backData.getChildByPath("type"));

            X = DataTool.getInt(backData.getChildByPath("x"));
            Y = DataTool.getInt(backData.getChildByPath("y"));
            RX = DataTool.getInt(backData.getChildByPath("rx"));
            RY = DataTool.getInt(backData.getChildByPath("ry"));
            CX = DataTool.getInt(backData.getChildByPath("cx"));
            CY = DataTool.getInt(backData.getChildByPath("cy"));
            A = DataTool.getInt(backData.getChildByPath("a"));
            BS = DataTool.getString(backData.getChildByPath("bS"));

            Front = DataTool.getInt(backData.getChildByPath("front"));
            F = DataTool.getInt(backData.getChildByPath("f"));
            Ani = DataTool.getInt(backData.getChildByPath("ani"));
            No = DataTool.getInt(backData.getChildByPath("no"));
        }

        public int Id { get; set; }
        public int MapId { get; set; }
        public int Index { get; set; }
        public string? BS { get; set; }
        public int No { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int RX { get; set; }
        public int RY { get; set; }
        public int Type { get; set; }
        public int CX { get; set; }
        public int CY { get; set; }
        public int A { get; set; }
        public int Front { get; set; }
        public int Ani { get; set; }
        public int F { get; set; }
    }
}
