using BaseProto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using SyncProto;

namespace Application.Core.Login.Servers
{
    internal class DataGrpcService : ServiceProto.DataService.DataServiceBase
    {
        readonly MasterServer _server;

        public DataGrpcService(MasterServer server)
        {
            _server = server;
        }

        public override Task<BoolWrapper> IsGuildQueued(QuildRequest request, ServerCallContext context)
        {
            return Task.FromResult(new BoolWrapper { Value = _server.IsGuildQueued(request.GuildId) });
        }

        public override Task<Empty> PutGuildQueued(QuildRequest request, ServerCallContext context)
        {
            _server.PutGuildQueued(request.GuildId);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> RemoveGuildQueued(QuildRequest request, ServerCallContext context)
        {
            _server.RemoveGuildQueued(request.GuildId);
            return Task.FromResult(new Empty());
        }
    }
}
