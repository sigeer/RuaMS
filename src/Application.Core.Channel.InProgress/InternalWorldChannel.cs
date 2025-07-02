using Application.Core.Login.Servers;
using Config;

namespace Application.Core.Channel.InProgress
{
    public sealed class InternalWorldChannel : ChannelServerWrapper
    {
        public InternalWorldChannel(WorldChannelServer worldChannel, List<WorldChannel> channels) : base(worldChannel.ServerName, channels.Select(x => x.ChannelConfig).ToList())
        {
            ChannelServer = worldChannel;
        }

        public WorldChannelServer ChannelServer { get; }

        public override void BroadcastMessage(string type, object message)
        {
            ChannelServer.OnMessageReceived(type, message);
        }
    }
}
