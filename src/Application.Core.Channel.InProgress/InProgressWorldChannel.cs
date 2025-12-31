using Application.Core.Login.Servers;
using Application.Shared.Servers;
using CreatorProto;
using ExpeditionProto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Channel.InProgress
{
    public sealed class InProgressWorldChannel : ChannelServerNode
    {
        public InProgressWorldChannel(WorldChannelServer worldChannel, List<ChannelConfig> channels)
        {
            ServerName = worldChannel.ServerName;
            ServerHost = worldChannel.ServerConfig.ServerHost;
            ServerConfigs = channels;
            ChannelServer = worldChannel;
        }

        public WorldChannelServer ChannelServer { get; }

        public override async Task SendMessage<TMessage>(int type, TMessage message, CancellationToken cancellationToken = default)
        {
            await ChannelServer.MessageDispatcherV.DispatchAsync(type, message.ToByteString(), cancellationToken);
        }

        public override async Task SendMessage(int type, CancellationToken cancellationToken = default)
        {
            await SendMessage(type, new Empty(), cancellationToken);
        }
    }
}
