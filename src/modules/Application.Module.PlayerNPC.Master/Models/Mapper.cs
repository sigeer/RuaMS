using Application.EF.Entities;
using AutoMapper;

namespace Application.Module.PlayerNPC.Master.Models
{
    internal class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<PlayerNpcEquipModel, PlayerNPCProto.PlayerNPCEquip>()
                .ForMember(dest => dest.ItemId, src => src.MapFrom(x => x.Equipid))
                .ForMember(dest => dest.Position, src => src.MapFrom(x => x.Equippos))
                .ReverseMap()
                .ForMember(dest => dest.Equipid, src => src.MapFrom(x => x.ItemId))
                .ForMember(dest => dest.Equippos, src => src.MapFrom(x => x.Position))
                ;
            CreateMap<PlayerNpcModel, PlayerNPCProto.PlayerNPCDto>()
                .ReverseMap();

            CreateMap<PlayerNpcEntity, PlayerNpcModel>()
                .ReverseMap();
            CreateMap<PlayerNpcsEquipEntity, PlayerNpcEquipModel>()
                .ReverseMap();
        }
    }
}
