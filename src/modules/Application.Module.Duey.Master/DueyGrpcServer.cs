using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Application.Module.Duey.Master
{
    internal class DueyGrpcServer : ProtoService.DueyService.DueyServiceBase
    {
        readonly DueyManager _manager;

        public DueyGrpcServer(DueyManager manager)
        {
            _manager = manager;
        }

        public override Task<ProtoService.CreatePackageResponse> CreateDueyPackage(ProtoService.CreatePackageRequest request, ServerCallContext context)
        {
            return Task.FromResult(_manager.CreateDueyPackage(request));
        }

        public override Task<ProtoService.GetPlayerDueyPackageResponse> GetPlayerDueyPackage(ProtoService.GetPlayerDueyPackageRequest request, ServerCallContext context)
        {
            return Task.FromResult(_manager.GetPlayerDueyPackages(request));
        }

        public override Task<Empty> RemoveDueyPackage(ProtoService.RemovePackageRequest request, ServerCallContext context)
        {
            _manager.RemovePackage(request);
            return base.RemoveDueyPackage(request, context);
        }

        public override Task<Empty> TakeDueyPackage(ProtoService.TakeDueyPackageRequest request, ServerCallContext context)
        {
            _manager.TakeDueyPackage(request);
            return base.TakeDueyPackage(request, context);
        }

        public override Task<Empty> TakeDueyPackageCommit(ProtoModel.TakeDueyPackageCommitProto request, ServerCallContext context)
        {
            _manager.TakeDueyPackageCommit(request);
            return base.TakeDueyPackageCommit(request, context);
        }

    }
}
