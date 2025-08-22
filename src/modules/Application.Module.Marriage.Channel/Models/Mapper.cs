using Application.Module.Marriage.Common.Models;
using Mapster;

namespace Application.Module.Marriage.Channel.Models
{
    internal class Mapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MarriageProto.WeddingInfoDto, WeddingInfo>();

            config.NewConfig<MarriageProto.MarriageDto, MarriageInfo>()
                .Map(dest => dest.Status, x => (MarriageStatusEnum)x.Status);
        }
    }
}
