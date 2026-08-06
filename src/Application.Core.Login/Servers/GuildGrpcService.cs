using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Application.Core.Login.Servers
{
    internal class GuildGrpcService : ProtoService.GuildService.GuildServiceBase
    {
        readonly MasterServer _server;

        public GuildGrpcService(MasterServer server)
        {
            _server = server;
        }


        public override Task<ProtoService.GetGuildResponse> GetGuildModel(ProtoService.GetGuildRequest request, ServerCallContext context)
        {
            return Task.FromResult(new ProtoService.GetGuildResponse { Model = _server.GuildManager.GetGuildFull(request.Id) });
        }

        public override Task<ProtoService.QueryRankedGuildsResponse> GetGuildRank(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.GuildManager.LoadRankedGuilds());
        }

        public override async Task<Empty> GuildDropMessage(ProtoService.GuildDropMessageRequest request, ServerCallContext context)
        {
            await _server.GuildManager.SendGuildMessage(request.GuildId, request.Type, request.Message);
            return await base.GuildDropMessage(request, context);
        }

        public override async Task<Empty> SendGuildPacket(ProtoService.GuildPacketRequest request, ServerCallContext context)
        {
            await _server.GuildManager.SendGuildPacket(request);
            return await base.SendGuildPacket(request, context);
        }
    }
}
