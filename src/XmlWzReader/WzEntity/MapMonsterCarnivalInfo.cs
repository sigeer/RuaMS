using System.Drawing;

namespace XmlWzReader.WzEntity
{
    public class MapMonsterCarnivalInfo
    {
        public MapMonsterCarnivalInfo(int mapId, Data cpqInfo)
        {
            MapId = mapId;

            DeathCP = (DataTool.getIntConvert("deathCP", cpqInfo, 0));
            MaxMobs = (DataTool.getIntConvert("mobGenMax", cpqInfo, 20));
            TimeDefault = (DataTool.getIntConvert("timeDefault", cpqInfo, 0));
            TimeExpand = (DataTool.getIntConvert("timeExpand", cpqInfo, 0));
            MaxReactors = (DataTool.getIntConvert("guardianGenMax", cpqInfo, 16));

            GuardianSpawnPoints = [];
            var guardianGenData = cpqInfo!.getChildByPath("guardianGenPos");
            foreach (Data node in guardianGenData.getChildren())
            {
                GuardianSpawnPoints.Add(new SpawnPointInfo(new Point(DataTool.getIntConvert("x", node), DataTool.getIntConvert("y", node)), DataTool.getIntConvert("team", node, -1)));
            }

            Skills = new List<int>();
            var cpqSkillData = cpqInfo.getChildByPath("skill");
            if (cpqSkillData != null)
            {
                foreach (var area in cpqSkillData)
                {
                    Skills.Add(DataTool.getInt(area));
                }
            }

            CPQMobDataList = new List<CPQMobData>();
            var cpqMobData = cpqInfo.getChildByPath("mob");
            if (cpqMobData != null)
            {
                foreach (var area in cpqMobData)
                {
                    CPQMobDataList.Add(new CPQMobData(DataTool.getInt(area.getChildByPath("id")), DataTool.getInt(area.getChildByPath("spendCP"))));
                }
            }
        }

        public int Id { get; set; }
        public int MapId { get; set; }
        public int DeathCP { get; private set; }
        public int MaxMobs { get; private set; }
        public int TimeDefault { get; private set; }
        public int TimeExpand { get; private set; }
        public int MaxReactors { get; private set; }

        public List<SpawnPointInfo> GuardianSpawnPoints { get; set; }
        public List<int> Skills { get; set; }
        public List<CPQMobData> CPQMobDataList { get; set; }
    }

    public record SpawnPointInfo(Point Point, int Team);
    public record CPQMobData(int Id, int CP);

}
