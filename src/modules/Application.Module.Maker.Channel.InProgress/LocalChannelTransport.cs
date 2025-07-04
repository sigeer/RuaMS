using MakerProto;

namespace Application.Module.Maker.Channel.InProgress
{
    internal class LocalChannelTransport : IChannelTransport
    {
        readonly Master.MakerManager _masterManager;

        public LocalChannelTransport(Master.MakerManager masterManager)
        {
            _masterManager = masterManager;
        }

        public QueryMakerCraftTableResponse GetMakerCraftTable(ItemIdRequest request)
        {
            return _masterManager.GetMakerCraftTable(request);
        }

        public MakerRequiredItems GetMakerDisassembledItems(ItemIdRequest request)
        {
            return _masterManager.GetMakerDisassembledItems(request);
        }

        public QueryMakerItemStatResponse GetMakerReagentStatUpgrade(ItemIdRequest request)
        {
            return _masterManager.GetMakerReagentStatUpgrade(request);
        }
    }
}
