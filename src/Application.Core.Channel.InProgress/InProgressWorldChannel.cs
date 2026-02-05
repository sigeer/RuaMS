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
            ServerName = worldChannel.InstanceName;
            ServerHost = worldChannel.ServerConfig.ServerHost;
            ServerConfigs = channels;
            ChannelServer = worldChannel;
        }

        public WorldChannelServer ChannelServer { get; }

        public override Task SendMessage<TMessage>(int type, TMessage message)
        {
            ChannelServer.MessageDispatcherV.DispatchAsync(type, message.ToByteString());
            return Task.CompletedTask;
        }

        public override async Task SendMessage(int type)
        {
            await SendMessage(type, new Empty());
        }
    }
}
