using Application.Shared.Team;
using Grpc.Core;
using TeamProto;

namespace Application.Core.Login.Servers
{
    internal class TeamGrpcService : ServiceProto.TeamService.TeamServiceBase
    {
        readonly MasterServer _server;

        public TeamGrpcService(MasterServer server)
        {
            _server = server;
        }

        public override Task<GetTeamResponse> GetTeamModel(GetTeamRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetTeamResponse { Model = _server.TeamManager.GetTeamDto(request.Id) });
        }
    }
}
