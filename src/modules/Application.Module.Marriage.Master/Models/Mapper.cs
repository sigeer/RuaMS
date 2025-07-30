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

            CreateMap<MarriageModel, MarriageProto.MarriageDto>()
                .ForMember(dest => dest.HusbandId, src => src.MapFrom(x => x.Husbandid))
                .ForMember(dest => dest.WifeId, src => src.MapFrom(x => x.Wifeid))
                .ForMember(dest => dest.HusbandName, src => src.MapFrom<MarriageHusbandNameValueResolver>())
                .ForMember(dest => dest.WifeName, src => src.MapFrom<MarriageWifeNameValueResolver>());
        }
    }
}
