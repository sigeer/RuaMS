using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Application.Core.Login.Servers
{
    internal class DataGrpcService : ProtoService.DataService.DataServiceBase
    {
        readonly MasterServer _server;

        public DataGrpcService(MasterServer server)
        {
            _server = server;
        }

        public override Task<ProtoModel.BoolWrapper> IsGuildQueued(ProtoService.GuildQueueRequest request, ServerCallContext context)
        {
            return Task.FromResult(new ProtoModel.BoolWrapper { Value = _server.IsGuildQueued(request.GuildId) });
        }

        public override Task<Empty> PutGuildQueued(ProtoService.GuildQueueRequest request, ServerCallContext context)
        {
            _server.PutGuildQueued(request.GuildId);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> RemoveGuildQueued(ProtoService.GuildQueueRequest request, ServerCallContext context)
        {
            _server.RemoveGuildQueued(request.GuildId);
            return Task.FromResult(new Empty());
        }
    }
}
