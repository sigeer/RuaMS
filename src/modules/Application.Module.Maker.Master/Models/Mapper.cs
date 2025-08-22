using Application.EF.Entities;
using Mapster;

namespace Application.Module.Maker.Master.Models
{
    public class Mapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MakerCreatedataEntity, MakerProto.MakerCraftTable>()
                .Map(dest => dest.ItemId, x => x.Itemid);
            config.NewConfig<MakerRecipedataEntity, MakerProto.MakerRequiredItem>()
                .Map(dest => dest.ItemId, x => x.ReqItem);

            config.NewConfig<MakerReagentdataEntity, MakerProto.MakerItemStat>()
                .Map(dest => dest.ItemId, x => x.Itemid);
        }
    }
}
