using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GuildProto;
using MessageProto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Login.Servers
{
    internal class GuildGrpcService: ServiceProto.GuildService.GuildServiceBase
    {
        readonly MasterServer _server;

        public GuildGrpcService(MasterServer server)
        {
            _server = server;
        }

        public override Task<GetGuildResponse> CreateGuild(CreateGuildRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetGuildResponse { Model = _server.GuildManager.CreateGuild(request.Name, request.LeaderId, request.Members.ToArray()) });
        }

        public override Task<GetGuildResponse> GetGuildModel(GetGuildRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetGuildResponse { Model = _server.GuildManager.GetGuildFull(request.Id)});
        }

        public override Task<QueryRankedGuildsResponse> GetGuildRank(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.GuildManager.LoadRankedGuilds());
        }

        public override Task<Empty> GuildDropMessage(GuildDropMessageRequest request, ServerCallContext context)
        {
            _server.GuildManager.SendGuildMessage(request.GuildId, request.Type, request.Message);
            return base.GuildDropMessage(request, context);
        }

        public override Task<Empty> SendGuildPacket(GuildPacketRequest request, ServerCallContext context)
        {
            _server.GuildManager.SendGuildPacket(request);
            return base.SendGuildPacket(request, context);
        }
    }
}
