using Application.Core.Channel;
using BaseProto;
using CreatorProto;
using ExpeditionProto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using SystemProto;

namespace Application.Host.Channel
{
    public class WorldChannelGrpcServer : ServiceProto.Master2ChannelService.Master2ChannelServiceBase
    {
        readonly WorldChannelServer _server;

        public WorldChannelGrpcServer(WorldChannelServer server)
        {
            _server = server;
        }

        public override Task<Empty> BroadcastMesssage(MessageWrapper request, ServerCallContext context)
        {
            _server.OnMessageReceived(request);
            return Task.FromResult(new Empty());
        }

        public override Task<CreateCharResponseDto> CreateCharacterFromChannel(CreateCharRequestDto request, ServerCallContext context)
        {
            return Task.FromResult(_server.DataService.CreatePlayer(request));
        }

        public override Task<QueryChannelExpedtionResponse> GetExpeditionInfo(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.DataService.GetExpeditionInfo());
        }
    }
}
