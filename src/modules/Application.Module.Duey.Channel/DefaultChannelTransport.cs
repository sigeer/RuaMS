using Application.Core.Channel;
using DueyDto;
using Grpc.Net.Client;

namespace Application.Module.Duey.Channel
{
    internal class DefaultChannelTransport : IChannelTransport
    {
        readonly DueyService.ChannelService.ChannelServiceClient _grpcClient;

        public DefaultChannelTransport(WorldChannelServer server)
        {
            _grpcClient = new DueyService.ChannelService.ChannelServiceClient(GrpcChannel.ForAddress(server.ServerConfig.MasterServerGrpcAddress));
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

        public void TakeDueyPackage(TakeDueyPackageRequest request)
        {
            _grpcClient.TakeDueyPackage(request);
        }
    }
}
