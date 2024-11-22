namespace Application.Host.Models
{
    public class ServerInfoDto
    {
        public bool IsOnline { get; set; }
        public int RunningWorldCount { get; set; }
        /// <summary>
        /// 0. 未运行  1. 启动中 2. 运行中
        /// </summary>
        public int State { get; set; }
    }
    public class WorldServerDto
    {
        public int Id { get; set; }

        public bool Enable { get; set; }

        public List<WorldChannelServerDto> Channels { get; set; } = [];

        /// <summary>
        /// 运行时有值
        /// </summary>
        public WorldServerConfig? ActualConfig { get; set; }
        /// <summary>
        /// 数据库设定值
        /// </summary>
        public WorldServerConfig Config { get; set; } = null!;
    }

    public class WorldServerConfig
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int StartPort { get; set; }
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
        public int ChannelCount { get; set; }
    }

    public class WorldChannelServerDto
    {
        public int Id { get; set; }
        public int Port { get; set; }
        public bool IsRunning { get; set; }
    }

    public class WorldServerState
    {
        public int Id { get; set; }
        public bool Enable { get; set; }
    }
}
