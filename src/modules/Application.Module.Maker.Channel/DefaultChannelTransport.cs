using Application.Core.Channel;
using Grpc.Net.Client;
using MakerProto;

namespace Application.Module.Maker.Channel
{
    internal class DefaultChannelTransport : IChannelTransport
    {
        readonly MakerService.ChannelService.ChannelServiceClient _grpcClient;
        public DefaultChannelTransport(WorldChannelServer server)
        {
            _grpcClient = new MakerService.ChannelService.ChannelServiceClient(GrpcChannel.ForAddress(server.ServerConfig.MasterServerGrpcAddress));
        }

        public QueryMakerCraftTableResponse GetMakerCraftTable(ItemIdRequest request)
        {
            return _grpcClient.GetMakerCraftTable(request);
        }

        public MakerProto.MakerRequiredItems GetMakerDisassembledItems(ItemIdRequest request)
        {
            return _grpcClient.GetMakerDisassembledItems(request);
        }

        public QueryMakerItemStatResponse GetMakerReagentStatUpgrade(ItemIdRequest request)
        {
            return _grpcClient.GetMakerReagentStatUpgrade(request);
        }
    }
}
