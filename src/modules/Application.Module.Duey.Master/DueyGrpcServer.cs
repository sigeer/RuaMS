using DueyDto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Application.Module.Duey.Master
{
    internal class DueyGrpcServer : DueyService.ChannelService.ChannelServiceBase
    {
        readonly DueyManager _manager;

        public DueyGrpcServer(DueyManager manager)
        {
            _manager = manager;
        }

        public override Task<Empty> CreateDueyPackage(CreatePackageRequest request, ServerCallContext context)
        {
            _manager.CreateDueyPackage(request);
            return base.CreateDueyPackage(request, context);
        }

        public override Task<GetPlayerDueyPackageResponse> GetPlayerDueyPackage(GetPlayerDueyPackageRequest request, ServerCallContext context)
        {
            var res = new GetPlayerDueyPackageResponse();
            res.List.AddRange(_manager.GetPlayerDueyPackages(request));
            res.ReceiverId = request.ReceiverId;
            return Task.FromResult(res);
        }

        public override Task<Empty> RemoveDueyPackage(RemovePackageRequest request, ServerCallContext context)
        {
            _manager.RemovePackageById(request);
            return base.RemoveDueyPackage(request, context);
        }

        public override Task<Empty> TakeDueyPackage(TakeDueyPackageRequest request, ServerCallContext context)
        {
            _manager.TakeDueyPackage(request);
            return base.TakeDueyPackage(request, context);
        }

        public override Task<Empty> TakeDueyPackageCommit(TakeDueyPackageCommit request, ServerCallContext context)
        {
            _manager.TakeDueyPackageCommit(request);
            return base.TakeDueyPackageCommit(request, context);
        }
    }
}
