namespace Application.Templates.Map
{
    public sealed class MapTemplate : AbstractTemplate, ILinkTemplate<MapTemplate>
    {
        public int CreateMobInterval { get; set; } = 5000;
        public int VRTop { get; set; }
        public int VRLeft { get; set; }
        public int VRBottom { get; set; }
        public int VRRight { get; set; }
        public bool Everlast { get; set; }
        public int ForcedReturn { get; set; }

        public int ReturnMap { get; set; }


        public bool FlyMap { get; set; }

        public bool Town { get; set; }

        public string? OnFirstUserEnter { get; set; }

        public string? OnUserEnter { get; set; }


        public int FieldType { get; set; }

        public int DecHP { get; set; }

        public int DecMP { get; set; }

        public int DecInterval { get; set; }

        public int ProtectItem { get; set; }

        public int FieldLimit { get; set; }

        public int TimeLimit { get; set; } = -1;

        public bool HasClock { get; set; }
        public bool HasShip { get; set; }
        public int FixedMobCapacity { get; set; } = 500;
        public float RecoveryRate { get; set; } = 1;
        public int SeatCount { get; set; }
        public MapPortalTemplate[] Portals { get; set; }

        public MapReactorTemplate[] Reactors { get; set; }


        public MapLifeTemplate[] Life { get; set; }

        public MapFootholdTemplate[] Footholds { get; set; }
        public MapBackTemplate[] Backs { get; set; }
        public MapAreaTemplate[] Areas { get; set; }
        public MapMonsterCarnivalTemplate? MonsterCarnival { get; set; }
        public MapSnowballTemplate? Snowball { get; set; }
        public MapCoconutTemplate? Coconut { get; set; }
        [WZPath("info/timeMob")]
        public MapTimeMobTemplate? TimeMob { get; set; }
        public MapMiniMapTemplate? MiniMap { get; set; }

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
