namespace Application.Shared.Servers
{

    public class ChannelConfig
    {
        public int Port { get; set; }
        public int MaxSize { get; set; } = 100;
    }

    public class ChannelServerConfig
    {
        /// <summary>
        /// 单进程部署可忽略
        /// 用于服务器内部交流的地址，建议使用内网IP
        /// </summary>
        public string MasterServerGrpcAddress { get; set; } = "http://192.168.0.1:7878";
        public int GrpcPort { get; set; } = 7879;
        public string ServerName { get; set; } = "Channel_Local";
        /// <summary>
        /// 供客户端使用的Host
        /// </summary>
        public string ServerHost { get; set; } = "127.0.0.1";
        public List<ChannelConfig> ChannelConfig { get; set; } = [];
        public ChannelServerSystemConfig SystemConfig { get; set; } = new ChannelServerSystemConfig();
    }

    public class RegisteredChannelConfig: ChannelConfig
    {
        public string ServerHost { get; set; } = "127.0.0.1";
        public string ServerName { get; set; } = null!;
    }

    public class ChannelServerSystemConfig
    {
        /// <summary>
        /// 自动清理不活跃地图
        /// </summary>
        public bool AutoClearMap { get; set; }
    }
}
