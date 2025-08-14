using Application.Shared.Servers;
using Config;
using CreatorProto;
using ExpeditionProto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using SystemProto;

namespace Application.Core.Login.Servers
{
    public abstract class ChannelServerWrapper
    {
        protected ChannelServerWrapper(string serverName, string serverHost, List<ChannelConfig> serverConfigs)
        {
            ServerName = serverName;
            ServerHost = serverHost;
            ServerConfigs = serverConfigs;
        }
        public string ServerHost { get; }
        public string ServerName { get; protected set; }
        public List<ChannelConfig> ServerConfigs { get; }

        public abstract void BroadcastMessage<TMessage>(string type, TMessage message) where TMessage : IMessage;
        public abstract CreatorProto.CreateCharResponseDto CreateCharacterFromChannel(CreatorProto.CreateCharRequestDto request);
        public abstract ExpeditionProto.QueryChannelExpedtionResponse GetExpeditionInfo();
    }


    public class RemoteWorldChannel : ChannelServerWrapper
    {
        readonly ServiceProto.Master2ChannelService.Master2ChannelServiceClient _client;
        public RemoteWorldChannel(string serverName, string serverHost, string grpcHost, int grpcPort, List<ChannelConfig> channelConfigs) : base(serverName, serverHost, channelConfigs)
        {
            _client = new ServiceProto.Master2ChannelService.Master2ChannelServiceClient(GrpcChannel.ForAddress($"http://{grpcHost}:{grpcPort}"));
        }

        public override void BroadcastMessage<TMessage>(string type, TMessage message)
        {
            _client.BroadcastMesssage(new BaseProto.MessageWrapper { Type = type, Content = message.ToByteString() });
        }

        public override CreateCharResponseDto CreateCharacterFromChannel(CreateCharRequestDto request)
        {
            return _client.CreateCharacterFromChannel(request);
        }

        public override QueryChannelExpedtionResponse GetExpeditionInfo()
        {
            return _client.GetExpeditionInfo(new Google.Protobuf.WellKnownTypes.Empty());
        }
    }
}
