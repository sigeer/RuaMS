using Application.EF.Entities;
using AutoMapper;

namespace Application.Module.Maker.Master.Models
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<MakerCreatedataEntity, MakerProto.MakerCraftTable>()
                .ForMember(dest => dest.ItemId, src => src.MapFrom(x => x.Itemid)); 
            CreateMap<MakerRecipedataEntity, MakerProto.MakerRequiredItem>()
                .ForMember(dest => dest.ItemId, src => src.MapFrom(x => x.ReqItem));

            CreateMap<MakerReagentdataEntity, MakerProto.MakerItemStat>()
                .ForMember(dest => dest.ItemId, src => src.MapFrom(x => x.Itemid)); 
        }
    }
}
