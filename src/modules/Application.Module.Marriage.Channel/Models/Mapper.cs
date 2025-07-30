using Application.Module.Marriage.Common.Models;
using AutoMapper;

namespace Application.Module.Marriage.Channel.Models
{
    internal class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<MarriageProto.WeddingInfoDto, WeddingInfo>();

            CreateMap<MarriageProto.MarriageDto, MarriageInfo>()
                .ForMember(dest => dest.Status, src => src.MapFrom(x => (MarriageStatusEnum)x.Status));
        }
    }
}
