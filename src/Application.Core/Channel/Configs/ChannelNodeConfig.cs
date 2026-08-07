using Application.Shared.Servers;

namespace Application.Core.Channel.Configs
{
    public class ChannelNodeConfig: INodeServer
    {
        public string ServerName { get; set; } = "LocalNode";
        /// <summary>
        /// 供客户端使用的Host
        /// </summary>
        public string ServerHost { get; set; } = "127.0.0.1";
        public List<ChannelConfig> ChannelConfigs { get; set; } = [];
        public ChannelServerSystemConfig SystemConfig { get; set; } = new ChannelServerSystemConfig();
    }
}
