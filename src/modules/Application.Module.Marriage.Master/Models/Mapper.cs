using Application.EF.Entities;
using Mapster;

namespace Application.Module.Marriage.Master.Models
{
    internal class Mapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<WeddingInfo, MarriageProto.WeddingInfoDto>()
                .Map(dest => dest.MarriageId, src => src.Id);

            config.NewConfig<MarriageEntity, MarriageModel>()
                .Map(dest => dest.Id, src => src.Marriageid);

            config.NewConfig<MarriageModel, MarriageEntity>()
                .Map(dest => dest.Marriageid, src => src.Id);

            config.NewConfig<MarriageModel, MarriageProto.MarriageDto>()
                .Map(dest => dest.HusbandId, src => src.Husbandid)
                .Map(dest => dest.WifeId, src => src.Wifeid);
        }
    }
}
