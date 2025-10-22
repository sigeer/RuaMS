namespace Application.Templates.Map
{
    //[GenerateTag] // 代码生成器没有适配foothold的格式
    public sealed class MapTemplate : AbstractTemplate, ILinkTemplate<MapTemplate>
    {
        [WZPath("info/mobRate")]
        public float MobRate { get; set; }
        [WZPath("info/createMobInterval")]
        public int CreateMobInterval { get; set; } = 5000;
        [WZPath("info/VRTop")]
        public int VRTop { get; set; }
        [WZPath("info/VRLeft")]
        public int VRLeft { get; set; }
        [WZPath("info/VRBottom")]
        public int VRBottom { get; set; }
        [WZPath("info/VRRight")]
        public int VRRight { get; set; }
        [WZPath("info/everlast")]
        public bool Everlast { get; set; }
        [WZPath("info/forcedReturn")]
        public int ForcedReturn { get; set; }
        [WZPath("info/returnMap")]
        public int ReturnMap { get; set; }

        [WZPath("info/fly")]
        public bool FlyMap { get; set; }
        [WZPath("info/town")]
        public bool Town { get; set; }
        [WZPath("info/onFirstUserEnter")]
        public string? OnFirstUserEnter { get; set; }
        [WZPath("info/onUserEnter")]
        public string? OnUserEnter { get; set; }

        [WZPath("info/fieldType")]
        public int FieldType { get; set; }
        [WZPath("info/decHP")]
        public int DecHP { get; set; }

        [WZPath("info/decInterval")]
        public int DecInterval { get; set; }
        [WZPath("info/protectItem")]
        public int ProtectItem { get; set; }
        [WZPath("info/fieldLimit")]
        public int FieldLimit { get; set; }
        [WZPath("info/timeLimit")]
        public int TimeLimit { get; set; } = -1;
        [WZPath("info/timeMob")]
        public MapTimeMobTemplate? TimeMob { get; set; }
        [WZPath("info/fixedMobCapacity")]
        public int FixedMobCapacity { get; set; } = 500;
        [WZPath("info/recovery")]
        public float RecoveryRate { get; set; } = 1;


        [WZPath("clock/$existed")]
        public bool HasClock { get; set; }
        [WZPath("shipObj/$existed")]
        public bool HasShip { get; set; }


        [WZPath("seat/$length")]
        public int SeatCount { get; set; }
        [WZPath("portal/-")]
        public MapPortalTemplate[] Portals { get; set; }
        [WZPath("reactor/-")]
        public MapReactorTemplate[] Reactors { get; set; }

        [WZPath("life/-")]
        public MapLifeTemplate[] Life { get; set; }
        [WZPath("foothold/-/-")]
        public MapFootholdTemplate[] Footholds { get; set; }
        [WZPath("back/-")]
        public MapBackTemplate[] Backs { get; set; }
        [WZPath("area/-")]
        public MapAreaTemplate[] Areas { get; set; }


        [WZPath("miniMap")]
        public MapMiniMapTemplate? MiniMap { get; set; }

        [WZPath("monsterCarnival")]
        public MapMonsterCarnivalTemplate? MonsterCarnival { get; set; }
        [WZPath("snowBall")]
        public MapSnowballTemplate? Snowball { get; set; }
        [WZPath("coconut")]
        public MapCoconutTemplate? Coconut { get; set; }
        public MapTemplate(int nMapId)
            : base(nMapId)
        {
            Portals = Array.Empty<MapPortalTemplate>();
            Reactors = Array.Empty<MapReactorTemplate>();
            Life = Array.Empty<MapLifeTemplate>();
            Footholds = Array.Empty<MapFootholdTemplate>();
            Backs = Array.Empty<MapBackTemplate>();
            Areas = Array.Empty<MapAreaTemplate>();
        }

        public void CloneLink(MapTemplate sourceTemplate)
        {
            sourceTemplate.Reactors = Reactors;
            sourceTemplate.Coconut = Coconut;
            sourceTemplate.Snowball = Snowball;
            sourceTemplate.Backs = Backs;
            sourceTemplate.Life = Life;
            sourceTemplate.Footholds = Footholds;
            sourceTemplate.MonsterCarnival = MonsterCarnival;
            sourceTemplate.HasClock = HasClock;
            sourceTemplate.HasShip = HasShip;
            sourceTemplate.MiniMap = MiniMap;
            sourceTemplate.Areas = Areas;
            sourceTemplate.SeatCount = SeatCount;
        }
    }
}
