using AllianceProto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Login.Servers
{
    internal class AllianceGrpcService: ServiceProto.AllianceService.AllianceServiceBase
    {
        readonly MasterServer _server;

        public AllianceGrpcService(MasterServer server)
        {
            _server = server;
        }

        public override Task<Empty> AllianceExpelGuild(AllianceExpelGuildRequest request, ServerCallContext context)
        {
            _server.GuildManager.AllianceExpelGuild(request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> ChangeLeader(AllianceChangeLeaderRequest request, ServerCallContext context)
        {
            _server.GuildManager.ChangeAllianceLeader(request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> ChangeMemberRank(ChangePlayerAllianceRankRequest request, ServerCallContext context)
        {
            _server.GuildManager.ChangePlayerAllianceRank(request);
            return Task.FromResult(new Empty());
        }

        public override Task<GetAllianceResponse> CreateAlliance(CreateAllianceRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetAllianceResponse { Model = _server.GuildManager.CreateAlliance(request.Members.ToArray(), request.Name)});
        }

        public override Task<CreateAllianceCheckResponse> CreateAllianceCheck(CreateAllianceCheckRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.GuildManager.CreateAllianceCheck(request));
        }

        public override Task<Empty> DisbandAlliance(DisbandAllianceRequest request, ServerCallContext context)
        {
            _server.GuildManager.DisbandAlliance(request);
            return Task.FromResult(new Empty());
        }

        public override Task<GetAllianceResponse> GetAllianceModel(GetAllianceRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetAllianceResponse { Model = _server.GuildManager.GetAllianceFull(request.Id)});
        }

        public override Task<Empty> IncreaseAllianceCapacity(IncreaseAllianceCapacityRequest request, ServerCallContext context)
        {
            _server.GuildManager.IncreaseAllianceCapacity(request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> JoinAlliance(GuildJoinAllianceRequest request, ServerCallContext context)
        {
            _server.GuildManager.GuildJoinAlliance(request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> LeavelAlliance(GuildLeaveAllianceRequest request, ServerCallContext context)
        {
            _server.GuildManager.GuildLeaveAlliance(request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SendAllianceChat(AllianceChatRequest request, ServerCallContext context)
        {
            _server.GuildManager.SendAllianceChat(request.Name, request.Text);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> UpdateNotice(UpdateAllianceNoticeRequest request, ServerCallContext context)
        {
            _server.GuildManager.UpdateAllianceNotice(request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> UpdateRankTitle(UpdateAllianceRankTitleRequest request, ServerCallContext context)
        {
            _server.GuildManager.UpdateAllianceRankTitle(request);
            return Task.FromResult(new Empty());
        }
    }
}
