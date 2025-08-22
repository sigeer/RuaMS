using Application.Core.Login.Datas;
using Application.EF.Entities;
using Mapster;

namespace Application.Module.Marriage.Master.Models
{
    internal class Mapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<WeddingInfo, MarriageProto.WeddingInfoDto>()
                .Map(dest => dest.MarriageId, x => x.Id);

            config.NewConfig<MarriageEntity, MarriageModel>()
                .TwoWays()
                .Map(dest => dest.Id, x => x.Marriageid);

            config.NewConfig<MarriageModel, MarriageProto.MarriageDto>()
                .Map(dest => dest.HusbandId, x => x.Husbandid)
                .Map(dest => dest.WifeId, x => x.Wifeid);
        }
    }
}
