using Application.Core.Login.Servers;
using Application.Shared.Servers;
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

        public override void BroadcastMessage<TMessage>(string type, TMessage message)
        {
            ChannelServer.OnMessageReceived(type, message);
        }

        public override CreatorProto.CreateCharResponseDto CreateCharacterFromChannel(CreatorProto.CreateCharRequestDto request)
        {
            return ChannelServer.DataService.CreatePlayer(request);
        }

        public override QueryChannelExpedtionResponse GetExpeditionInfo()
        {
            return ChannelServer.DataService.GetExpeditionInfo();
        }

        public override async Task SendMessage<TMessage>(int type, TMessage message, CancellationToken cancellationToken = default)
        {
            await ChannelServer.MessageDispatcherV.DispatchAsync(type, message.ToByteString(), cancellationToken);
        }

        public override async Task SendMessage(int type, CancellationToken cancellationToken = default)
        {
            await ChannelServer.MessageDispatcherV.DispatchAsync(type, new Empty().ToByteString(), cancellationToken);
        }
    }
}
