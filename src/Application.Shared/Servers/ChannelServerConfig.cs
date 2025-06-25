namespace Application.Shared.Servers
{

    public class ChannelConfig
    {
        public int Port { get; set; }
        public string Host { get; set; } = "127.0.0.1";
        public int MaxSize { get; set; } = 100;
    }

    public class ChannelServerConfig
    {
        /// <summary>
        /// 单进程部署可忽略
        /// 用于服务器内部交流的地址，建议使用内网IP
        /// </summary>
        public string? GrpcServiceEndPoint { get; set; } = "192.168.0.1:7878";
        public string ServerName { get; set; } = "Channel_Local";
        public List<ChannelConfig> ChannelConfig { get; set; } =
            [
                new ChannelConfig(){ Port = 7575},
                new ChannelConfig(){ Port = 7576},
                new ChannelConfig(){ Port = 7577}
            ];
    }

    public class RegisteredChannelConfig: ChannelConfig
    {
        public string ServerName { get; set; } = null!;
    }
}
