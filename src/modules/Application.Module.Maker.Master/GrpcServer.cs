using Grpc.Core;
using MakerProto;

namespace Application.Module.Maker.Master
{
    internal class GrpcServer : MakerService.ChannelService.ChannelServiceBase
    {
        readonly MakerManager _manager;

        public GrpcServer(MakerManager manager)
        {
            _manager = manager;
        }

        public override Task<QueryMakerCraftTableResponse> GetMakerCraftTable(ItemIdRequest request, ServerCallContext context)
        {
            return Task.FromResult(_manager.GetMakerCraftTable(request));
        }

        public override Task<MakerRequiredItems> GetMakerDisassembledItems(ItemIdRequest request, ServerCallContext context)
        {
            return Task.FromResult(_manager.GetMakerDisassembledItems(request));
        }

        public override Task<QueryMakerItemStatResponse> GetMakerReagentStatUpgrade(ItemIdRequest request, ServerCallContext context)
        {
            return Task.FromResult(_manager.GetMakerReagentStatUpgrade(request));
        }
    }
}
