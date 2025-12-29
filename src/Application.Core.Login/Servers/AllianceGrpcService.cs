using AllianceProto;
using Grpc.Core;

namespace Application.Core.Login.Servers
{
    internal class AllianceGrpcService : ServiceProto.AllianceService.AllianceServiceBase
    {
        readonly MasterServer _server;

        public AllianceGrpcService(MasterServer server)
        {
            _server = server;
        }

        public override Task<GetAllianceResponse> CreateAlliance(CreateAllianceRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetAllianceResponse { Model = _server.GuildManager.CreateAlliance(request.Members.ToArray(), request.Name) });
        }

        public override Task<CreateAllianceCheckResponse> CreateAllianceCheck(CreateAllianceCheckRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.GuildManager.CreateAllianceCheck(request));
        }

        public override Task<GetAllianceResponse> GetAllianceModel(GetAllianceRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetAllianceResponse { Model = _server.GuildManager.GetAllianceFull(request.Id) });
        }
    }
}
