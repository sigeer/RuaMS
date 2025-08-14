using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GuildProto;
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

        public override Task<Empty> DisbandGuild(GuildDisbandRequest request, ServerCallContext context)
        {
            _server.GuildManager.DisbandGuild(request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> ExpelFromGuild(ExpelFromGuildRequest request, ServerCallContext context)
        {
            _server.GuildManager.GuildExpelMember(request);
            return Task.FromResult(new Empty());
        }

        public override Task<GetGuildResponse> GetGuildModel(GetGuildRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetGuildResponse { Model = _server.GuildManager.GetGuildFull(request.Id)});
        }

        public override Task<QueryRankedGuildsResponse> GetGuildRank(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.GuildManager.LoadRankedGuilds());
        }

        public override Task<Empty> JoinGuild(JoinGuildRequest request, ServerCallContext context)
        {
            _server.GuildManager.PlayerJoinGuild(request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> LeaveGuild(LeaveGuildRequest request, ServerCallContext context)
        {
            _server.GuildManager.PlayerLeaveGuild(request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SendGuildChat(GuildChatRequest request, ServerCallContext context)
        {
            _server.GuildManager.SendGuildChat(request.Name, request.Text);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> UpdateCapacity(UpdateGuildCapacityRequest request, ServerCallContext context)
        {
            _server.GuildManager.IncreseGuildCapacity(request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> UpdateEmblem(UpdateGuildEmblemRequest request, ServerCallContext context)
        {
            _server.GuildManager.UpdateGuildEmblem(request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> UpdateGP(UpdateGuildGPRequest request, ServerCallContext context)
        {
            _server.GuildManager.UpdateGuildGP(request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> UpdateMemberRank(UpdateGuildMemberRankRequest request, ServerCallContext context)
        {
            _server.GuildManager.ChangePlayerGuildRank(request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> UpdateNotice(UpdateGuildNoticeRequest request, ServerCallContext context)
        {
            _server.GuildManager.UpdateGuildNotice(request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> UpdateRankTitle(UpdateGuildRankTitleRequest request, ServerCallContext context)
        {
            _server.GuildManager.UpdateGuildRankTitle(request);
            return Task.FromResult(new Empty());
        }
    }
}
