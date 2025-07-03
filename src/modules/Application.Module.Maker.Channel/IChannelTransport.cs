namespace Application.Module.Maker.Channel
{
    public interface IChannelTransport
    {
        public MakerProto.QueryMakerCraftTableResponse GetMakerCraftTable(MakerProto.ItemIdRequest request);
        MakerProto.QueryMakerItemStatResponse GetMakerReagentStatUpgrade(MakerProto.ItemIdRequest request);
        BaseProto.ItemQuantityList GetMakerDisassembledItems(MakerProto.ItemIdRequest request);
    }
}
