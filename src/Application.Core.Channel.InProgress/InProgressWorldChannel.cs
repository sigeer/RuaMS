using Application.Core.Login.Servers;
using ExpeditionProto;
using Google.Protobuf.WellKnownTypes;
using SystemProto;

namespace Application.Core.Channel.InProgress
{
    public sealed class InProgressWorldChannel : ChannelServerNode
    {
        public InProgressWorldChannel(WorldChannelServer worldChannel, List<WorldChannel> channels) 
            : base(worldChannel.ServerName, worldChannel.ServerConfig.ServerHost, channels.Select(x => x.ChannelConfig).ToList())
        {
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
    }
}
