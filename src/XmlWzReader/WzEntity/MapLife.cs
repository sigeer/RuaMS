namespace XmlWzReader.WzEntity
{
    public class MapLife
    {
        public static bool IsCPQMap2(int mapId)
        {
            switch (mapId)
            {
                case 980031100:
                case 980032100:
                case 980033100:
                    return true;
            }
            return false;
        }
        public MapLife(int mapId, Data lifeData)
        {
            MapId = mapId;
            Index = int.Parse(lifeData.getName()!);

            LifeId = DataTool.getIntConvert(lifeData.getChildByPath("id"));
            Type = DataTool.getString(lifeData.getChildByPath("type")) ?? "m";
            Team = DataTool.getInt("team", lifeData, -1);
            if (IsCPQMap2(mapId) && Type == "m")
            {
                if ((Index % 2) == 0)
                {
                    Team = 0;
                }
                else
                {
                    Team = 1;
                }
            }
            Cy = DataTool.getInt(lifeData.getChildByPath("cy"));
            var dF = lifeData.getChildByPath("f");
            F = (dF != null) ? DataTool.getInt(dF) : 0;
            Fh = DataTool.getInt(lifeData.getChildByPath("fh"));
            Rx0 = DataTool.getInt(lifeData.getChildByPath("rx0"));
            Rx1 = DataTool.getInt(lifeData.getChildByPath("rx1"));
            X = DataTool.getInt(lifeData.getChildByPath("x"));
            Y = DataTool.getInt(lifeData.getChildByPath("y"));
            Hide = DataTool.getInt("hide", lifeData, 0) == 1;
            MobTime = DataTool.getInt("mobTime", lifeData, 0);
        }

        public int Id { get; set; }
        public int Index { get; set; }
        public int MapId { get; set; }
        public string Type { get; set; }
        public int LifeId { get; set; }
        public int Team { get; set; } = -1;
        public int X { get; set; }
        public int Y { get; set; }
        public int MobTime { get; set; }
        public int F { get; set; }
        public bool Hide { get; set; }
        public int Fh { get; set; }
        public int Cy { get; set; }
        public int Rx0 { get; set; }
        public int Rx1 { get; set; }

    }
}
