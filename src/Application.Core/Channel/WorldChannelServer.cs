using Application.Core.Channel.ServerData;
using Application.Core.ServerTransports;
using Application.Shared.Servers;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Channel
{
    public class WorldChannelServer
    {
        readonly IServiceProvider _sp;
        readonly IChannelServerTransport _transport;
        public Dictionary<int, WorldChannel> Servers { get; set; }
        /// <summary>
        /// 单进程部署可忽略
        /// 用于服务器内部交流的地址，建议使用内网IP
        /// </summary>
        public string? GrpcServiceEndPoint { get; set; } = "192.168.0.1:7878";

        public GuildManager GuildManager { get; private set; } = null!;

        public WorldChannelServer(IServiceProvider sp, IChannelServerTransport transport)
        {
            _sp = sp;
            _transport = transport;

            Servers = new();

        }

        public async Task Start(int startPort = 7574, int count = 3)
        {
            GuildManager = _sp.GetRequiredService<GuildManager>();

            // ping master
            for (int j = 1; j <= count; j++)
            {
                var config = new WorldChannelConfig
                {
                    Port = startPort + j
                };
                var scope = _sp.CreateScope();
                var channel = new WorldChannel(this, scope, config, _transport);
                await channel.StartServer();
                if (channel.IsRunning)
                {
                    Servers[channel.getId()] = channel;
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

        public IPlayer? FindPlayerById(int cid)
        {
            foreach (var item in Servers.Values)
            {
                var chr = item.Players.getCharacterById(cid);
                if (chr != null)
                    return chr;
            }
            return null;
        }

        public IPlayer? FindPlayerById(int channel, int cid)
        {
            if (Servers.TryGetValue(channel, out var ch))
                return ch.Players.getCharacterById(cid);

            return null;
        }

        internal WorldChannel? GetChannel(int channel)
        {
            return Servers.GetValueOrDefault(channel);
        }
    }
}
