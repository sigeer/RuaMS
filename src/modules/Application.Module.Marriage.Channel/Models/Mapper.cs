using AutoMapper;

namespace Application.Module.Marriage.Channel.Models
{
    internal class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<MarriageProto.WeddingInfoDto, WeddingInfo>();
        }
    }
}
