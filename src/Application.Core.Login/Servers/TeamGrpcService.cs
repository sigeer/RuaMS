using Grpc.Core;

namespace Application.Core.Login.Servers
{
    internal class TeamGrpcService : ProtoService.TeamService.TeamServiceBase
    {
        readonly MasterServer _server;

        public TeamGrpcService(MasterServer server)
        {
            _server = server;
        }

        public override Task<ProtoService.GetTeamResponse> GetTeamModel(ProtoService.GetTeamRequest request, ServerCallContext context)
        {
            return Task.FromResult(new ProtoService.GetTeamResponse { Model = _server.TeamManager.GetTeamDto(request.Id) });
        }
    }
}
