using Application.EF.Entities;
using Mapster;

namespace Application.Module.Maker.Master.Models
{
    public class Mapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MakerCreatedataEntity, MakerProto.MakerCraftTable>()
                .Map(dest => dest.ItemId, src => src.Itemid);
            config.NewConfig<MakerRecipedataEntity, MakerProto.MakerRequiredItem>()
                .Map(dest => dest.ItemId, src => src.ReqItem);

            config.NewConfig<MakerReagentdataEntity, MakerProto.MakerItemStat>()
                .Map(dest => dest.ItemId, src => src.Itemid);
        }
    }
}
