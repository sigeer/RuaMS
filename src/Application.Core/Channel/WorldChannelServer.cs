using Application.Core.Channel.ServerData;
using Application.Core.ServerTransports;
using Application.Shared.Servers;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.Xml;

namespace Application.Core.Channel
{
    public class WorldChannelServer
    {
        readonly IServiceProvider _sp;
        readonly IChannelServerTransport _transport;
        public List<WorldChannel> Servers { get; set; }
        /// <summary>
        /// 单进程部署可忽略
        /// 用于服务器内部交流的地址，建议使用内网IP
        /// </summary>
        public string? GrpcServiceEndPoint { get; set; } = "192.168.0.1:7878";

        public GuildManager GuildManager { get; }

        public WorldChannelServer(IServiceProvider sp, IChannelServerTransport transport)
        {
            _sp = sp;
            _transport = transport;

            Servers = new List<WorldChannel>();
            GuildManager = sp.GetRequiredService<GuildManager>();
        }

        public async Task Start(int startPort = 7574, int count = 3)
        {
            // ping master
            for (int j = 1; j <= count; j++)
            {
                var config = new WorldChannelConfig
                {
                    Port = startPort + j
                };
                var scope = _sp.CreateScope();
                var channel = new WorldChannel(scope, config, _transport);
                await channel.StartServer();
                if (channel.IsRunning)
                {
                    Servers.Add(channel);
                }
            }
        }

        //internal void SendDropGuildMessage(int guildId, int v, string message)
        //{
        //    _transport.SendDropGuildMessage(getId(), guildId, type, message);

        //    var guild = GuildManager.GetGuildById(guildId);
        //    if (guild != null)
        //    {
        //        guild.dropMessage(type, message);
        //    }
        //}
    }
}
