using Application.Core.Login.Servers;
using Application.Shared.Servers;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Channel.InProgress
{
    public sealed class InProgressNodeServer : AbstractChannelNodeServer
    {
        public InProgressNodeServer(WorldChannelServer worldChannel, List<ChannelConfig> channels)
        {
            ServerName = worldChannel.NodeConfig.ServerName;
            ServerHost = worldChannel.NodeConfig.ServerHost;
            ChannelConfigs = channels;
            ChannelServer = worldChannel;
        }

        public WorldChannelServer ChannelServer { get; }

        public override Task SendMessage<TMessage>(int type, TMessage message)
        {
            return ChannelServer.MessageDispatcherV.DispatchAsync(type, message.ToByteString());
        }

        public override async Task SendMessage(int type)
        {
            await SendMessage(type, new Empty());
        }
    }
}
