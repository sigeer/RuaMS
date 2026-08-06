using Application.Core.Channel;
using Grpc.Net.Client;

namespace Application.Module.Duey.Channel
{
    internal class DefaultChannelTransport : IChannelTransport
    {
        readonly ProtoService.DueyService.DueyServiceClient _grpcClient;

        public DefaultChannelTransport(ProtoService.DueyService.DueyServiceClient client)
        {
            _grpcClient = client;
        }

        public ProtoService.CreatePackageResponse CreateDueyPackage(ProtoService.CreatePackageRequest request)
        {
            return _grpcClient.CreateDueyPackage(request);
        }

        public ProtoService.GetPlayerDueyPackageResponse GetDueyPackagesByPlayerId(ProtoService.GetPlayerDueyPackageRequest request)
        {
            return _grpcClient.GetPlayerDueyPackage(request);
        }

        public void RequestRemovePackage(ProtoService.RemovePackageRequest request)
        {
            _grpcClient.RemoveDueyPackage(request);
        }

        public void TakeDueyPackage(ProtoService.TakeDueyPackageRequest request)
        {
            _grpcClient.TakeDueyPackage(request);
        }

        public void TakeDueyPackageCommit(ProtoModel.TakeDueyPackageCommitProto takeDueyPackageCommit)
        {
            _grpcClient.TakeDueyPackageCommit(takeDueyPackageCommit);
        }
    }
}
