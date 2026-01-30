using Application.Core.Channel;
using Grpc.Net.Client;
using MakerProto;

namespace Application.Module.Maker.Channel
{
    internal class DefaultChannelTransport : IChannelTransport
    {
        readonly MakerService.ChannelService.ChannelServiceClient _grpcClient;
        public DefaultChannelTransport(MakerService.ChannelService.ChannelServiceClient client)
        {
            _grpcClient = client;
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
