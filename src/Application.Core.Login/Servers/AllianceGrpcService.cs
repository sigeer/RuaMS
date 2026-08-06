using Grpc.Core;

namespace Application.Core.Login.Servers
{
    internal class AllianceGrpcService : ProtoService.AllianceService.AllianceServiceBase
    {
        readonly MasterServer _server;

        public AllianceGrpcService(MasterServer server)
        {
            _server = server;
        }

        public override Task<ProtoService.CreateAllianceCheckResponse> CreateAllianceCheck(ProtoService.CreateAllianceCheckRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.GuildManager.CreateAllianceCheck(request));
        }

        public override Task<ProtoService.GetAllianceResponse> GetAllianceModel(ProtoService.GetAllianceRequest request, ServerCallContext context)
        {
            return Task.FromResult(new ProtoService.GetAllianceResponse { Model = _server.GuildManager.GetAllianceDto(request.Id) });
        }
    }
}
