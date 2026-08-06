using Application.Core.Login;
using Application.Core.Login.Servers;
using Application.Host.Models;

namespace Application.Host.Services
{
    public class ServerService
    {
        readonly MasterServer _server;

        public ServerService(MasterServer server)
        {
            _server = server;
        }

        public ServerDashboard GetDashboard()
        {
            return new ServerDashboard
            {
                IsRunning = _server.IsRunning,
                Nodes = _server.ChannelServerList.Values.Select(x => new ServerNodeInfo
                {
                    Type = x is RemoteChannelServerNode ? Shared.Servers.NodeType.Remote : Shared.Servers.NodeType.InProgress,
                    Name = x.ServerName,
                    WanHost = x.ServerHost,
                    Channels = x.ServerConfigs.Select(y => new NodeChannelInfo
                    {
                        Port = y.Port
                    }).ToList()
                }).ToList()
            };
        }

        //public async Task StopAsync()
        //{
        //    await _server.Shutdown();
        //}

        //public async Task Start()
        //{
        //    await _server.StartServer();
        //}
    }
}
