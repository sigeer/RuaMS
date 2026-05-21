namespace Application.Shared.Servers
{

    public class ChannelConfig
    {
        public int Port { get; set; }
        public int MaxSize { get; set; } = 100;
    }

    public class ChannelServerConfig
    {
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
        public int ClientWidth { get; set; } = 1280;
        public int ClientHeight { get; set; } = 720;
        public double RangeOfConversation { get; set; } = 1000000;

        double _rangeOfVisibility = 0;
        public double GetRangedDistance()
        {
            if (_rangeOfVisibility <= 0)
            {
                if (ClientWidth <= 0 && ClientHeight <= 0)
                {
                    _rangeOfVisibility = double.PositiveInfinity;
                }
                else
                {
                    var radius = Math.Max(ClientHeight, ClientWidth) * 1.1;
                    _rangeOfVisibility = radius * radius;
                }
            }
            return _rangeOfVisibility;
        }
        
    }
}
