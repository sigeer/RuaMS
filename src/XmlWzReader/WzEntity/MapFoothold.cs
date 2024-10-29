namespace XmlWzReader.WzEntity
{
    public class MapFoothold
    {
        public MapFoothold(int mapId, Data footHoldData)
        {
            MapId = mapId;

            X1 = DataTool.getInt(footHoldData.getChildByPath("x1"));
            Y1 = DataTool.getInt(footHoldData.getChildByPath("y1"));
            X2 = DataTool.getInt(footHoldData.getChildByPath("x2"));
            Y2 = DataTool.getInt(footHoldData.getChildByPath("y2"));

            Index = int.Parse(footHoldData.getName()!);
            Prev = DataTool.getInt(footHoldData.getChildByPath("prev"));
            Next = DataTool.getInt(footHoldData.getChildByPath("next"));
        }

        public int Id { get; set; }
        public int MapId { get; set; }

        public int Index { get; set; }

        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }


        public int Prev { get; set; }
        public int Next { get; set; }
    }
}
