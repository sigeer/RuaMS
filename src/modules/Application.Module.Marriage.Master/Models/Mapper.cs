using Application.EF.Entities;
using AutoMapper;

namespace Application.Module.Marriage.Master.Models
{
    internal class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<WeddingInfo, MarriageProto.WeddingInfoDto>()
                .ForMember(dest => dest.MarriageId, src => src.MapFrom(x => x.Id))
                .ForMember(dest => dest.BrideName, src => src.MapFrom<WeddingBrideNameValueResolver>())
                .ForMember(dest => dest.GroomName, src => src.MapFrom<WeddingGroomNameValueResolver>());

            CreateMap<MarriageEntity, MarriageModel>()
                .ForMember(dest => dest.Id, src => src.MapFrom(x => x.Marriageid))
                .ReverseMap()
                .ForMember(dest => dest.Marriageid, src => src.MapFrom(x => x.Id));
        }
    }
}
