using Grpc.Core;

namespace Application.Core.Login.Servers
{
    internal class DueyGrpcService : ProtoService.DueyService.DueyServiceBase
    {
        readonly MasterServer _server;

        public DueyGrpcService(MasterServer server)
        {
            _server = server;
        }

        public override Task<ProtoService.CreatePackageResponse> CreateDueyPackage(ProtoService.CreatePackageRequest request, ServerCallContext context)
        {
            return _server.DueyManager.CreateDueyPackage(request);
        }
    }
}
