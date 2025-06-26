using DueyDto;
using Grpc.Net.Client;

namespace Application.Module.Duey.Channel
{
    internal class DefaultChannelTransport : IChannelTransport
    {
        readonly DueyService.ChannelService.ChannelServiceClient _grpcClient;

        public DefaultChannelTransport(GrpcChannel grpcChannel)
        {
            _grpcClient = new DueyService.ChannelService.ChannelServiceClient(grpcChannel);
        }

        public void CreateDueyPackage(CreatePackageRequest request)
        {
            _grpcClient.CreateDueyPackage(request);
        }

        public GetPlayerDueyPackageResponse GetDueyPackagesByPlayerId(GetPlayerDueyPackageRequest request)
        {
            return _grpcClient.GetPlayerDueyPackage(request);
        }

        public void RequestRemovePackage(RemovePackageRequest request)
        {
            _grpcClient.RemoveDueyPackage(request);
        }

        public void TakeDueyPackageCommit(TakeDueyPackageCommit request)
        {
            _grpcClient.TakeDueyPackageCommit(request);
        }

        public TakeDueyPackageResponse TakeDueyPackage(TakeDueyPackageRequest request)
        {
            return _grpcClient.TakeDueyPackage(request);
        }
    }
}
