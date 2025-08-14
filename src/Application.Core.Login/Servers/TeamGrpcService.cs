using Application.Shared.Team;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamProto;

namespace Application.Core.Login.Servers
{
    internal class TeamGrpcService: ServiceProto.TeamService.TeamServiceBase
    {
        readonly MasterServer _server;

        public TeamGrpcService(MasterServer server)
        {
            _server = server;
        }

        public override Task<GetTeamResponse> CreateTeam(CreateTeamRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetTeamResponse { Model = _server.TeamManager.CreateTeam(request.LeaderId) });
        }

        public override Task<GetTeamResponse> GetTeamModel(GetTeamRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetTeamResponse { Model = _server.TeamManager.GetTeamFull(request.Id) });
        }

        public override Task<Empty> SendTeamChat(TeamChatRequest request, ServerCallContext context)
        {
            _server.TeamManager.SendTeamChat(request.FromName, request.Text);
            return Task.FromResult(new Empty());
        }

        public override Task<UpdateTeamResponse> SendTeamUpdate(UpdateTeamRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.TeamManager.UpdateParty(request.TeamId, (PartyOperation)request.Operation, request.FromId, request.TargetId));
        }
    }
}
