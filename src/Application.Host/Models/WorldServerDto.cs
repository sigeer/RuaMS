namespace Application.Host.Models
{
    public class WorldServerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
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
        public List<WorldChannelServerDto> Channels { get; set; } = [];
    }

    public class WorldChannelServerDto
    {
        public int Id { get; set; }
        public int Port { get; set; }
        public bool IsRunning { get; set; }
    }
}
