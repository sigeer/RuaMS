using Application.Shared.Servers;

namespace Application.Host.Models
{
    public class ServerDashboard
    {
        public bool IsRunning { get; set; }
        public List<ServerNodeInfo> Nodes { get; set; } = [];
    }

    public class ServerNodeInfo
    {
        public string Name { get; set; }
        public NodeType Type { get; set; }
        /// <summary>
        /// 供服务器内部通信
        /// </summary>
        public string LanHost { get; set; }
        /// <summary>
        /// 供客户端连接
        /// </summary>
        public string WanHost { get; set; }
        public List<NodeChannelInfo> Channels { get; set; } = [];
    }

    public class NodeChannelInfo
    {
        public string Name { get; set; }
        public int Port { get; set; }
    }
}
