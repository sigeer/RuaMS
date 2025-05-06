namespace Application.Core.EF.Entities.SystemBase
{
    public class WorldConfigEntity
    {
        public WorldConfigEntity() { }
        public WorldConfigEntity(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        /// <summary>
        /// (0 = nothing, 1 = event, 2 = new, 3 = hot)
        /// </summary>
        public int Flag { get; set; }
        public bool Enable { get; set; }
        public string EventMessage { get; set; } = "";
        public string ServerMessage { get; set; } = "Welcome";
        public string RecommendMessage { get; set; } = "";
        public float QuestRate { get; set; } = 1;
        public float ExpRate { get; set; } = 1;
        public float MesoRate { get; set; } = 1;
        public float DropRate { get; set; } = 1;
        public float BossDropRate { get; set; } = 1;
        public float MobRate { get; set; } = 1;
        public float FishingRate { get; set; } = 1;
        public float TravelRate { get; set; } = 1;
        public int ChannelCount { get; set; } = 3;
        public int StartPort { get; set; }
    }
}
