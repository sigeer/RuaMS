namespace XmlWzReader.WzEntity
{
    public class MapInfo
    {
        public MapInfo(int mapId, Data infoData)
        {
            MapId = mapId;
            var mapStr = mapId.ToString();

            Town = DataTool.GetIntBool("town", infoData);
            Bgm = DataTool.getString("bgm", infoData);
            MapDescription = DataTool.getString("mapDesc", infoData);
            MapMark = DataTool.getString("mapMark", infoData);
            MoveLimit = DataTool.getIntConvert("moveLimit", infoData);

            Everlast = DataTool.GetIntBool("everlast", infoData);
            DecHP = DataTool.getIntConvert("decHP", infoData);
            ProtectItem = DataTool.getIntConvert("protectItem", infoData);

            VRTop = DataTool.getInt(infoData?.getChildByPath("VRTop"));
            VRBottom = DataTool.getInt(infoData?.getChildByPath("VRBottom"));
            VRLeft = DataTool.getInt(infoData?.getChildByPath("VRLeft"));
            VRRight = DataTool.getInt(infoData?.getChildByPath("VRRight"));

            ForceReturn = DataTool.getInt(infoData?.getChildByPath("forcedReturn"), 999999999);
            ReturnMap = DataTool.getInt(infoData?.getChildByPath("returnMap"));

            var onFirstUserEnter = DataTool.getString(infoData?.getChildByPath("onFirstUserEnter"));
            OnFirstUserEnter = string.IsNullOrEmpty(onFirstUserEnter) ? mapStr : onFirstUserEnter;

            var onUserEnter = DataTool.getString(infoData?.getChildByPath("onUserEnter"));
            OnUserEnter = string.IsNullOrEmpty(onUserEnter) ? mapStr : onUserEnter;

            CreateMobInterval = (short)DataTool.getInt(infoData?.getChildByPath("createMobInterval"), 5000);
            MobRate = DataTool.getFloat(infoData?.getChildByPath("mobRate"));
            FixedMobCapacity = DataTool.getIntConvert("fixedMobCapacity", infoData, 500);

            Recovery = DataTool.getFloat(infoData?.getChildByPath("recovery"));
            TimeLimit = DataTool.getIntConvert("timeLimit", infoData, -1);

            FieldLimit = DataTool.getInt(infoData?.getChildByPath("fieldLimit"));
            FieldType = DataTool.getIntConvert("fieldType", infoData);

            var timeMob = infoData?.getChildByPath("timeMob");
            if (timeMob != null)
            {
                TimeMob = new TimeMob(DataTool.getInt(timeMob.getChildByPath("id")), DataTool.getString(timeMob.getChildByPath("message")));
            }

        }
        public int Id { get; set; }
        public int MapId { get; set; }

        public int Version { get; set; }
        public int Cloud { get; set; }
        public string? MapDescription { get; set; }
        public bool Town { get; set; }
        public bool Everlast { get; set; }

        public int DecHP { get; set; }
        public int ProtectItem { get; set; }


        public int VRTop { get; set; }
        public int VRBottom { get; set; }
        public int VRLeft { get; set; } 
        public int VRRight { get; set; }

        public int ReturnMap { get; set; }
        public float MobRate { get; set; }
        public short CreateMobInterval { get; set; } = 5000;
        public string? Bgm { get; set; }
        public int ForceReturn { get; set; }
        public int MoveLimit { get; set; }
        public string? MapMark { get; set; }
        public int FieldLimit { get; set; }
        public int FieldType { get; set; }
        public int FixedMobCapacity { get; set; } = 500;

        public int TimeLimit { get; set; } = -1;

        public float Recovery { get; set; }

        public string OnFirstUserEnter { get; set; }
        public string OnUserEnter { get; set; }

        public TimeMob? TimeMob { get; set; }
    }

    public record TimeMob(int Id, string Message);
}
