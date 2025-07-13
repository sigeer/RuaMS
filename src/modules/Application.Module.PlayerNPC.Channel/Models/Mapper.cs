using AutoMapper;
using System.Drawing;

namespace Application.Module.PlayerNPC.Channel.Models
{
    internal class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<PlayerNPCProto.PlayerNPCEquip, PlayerNpcEquipObject>()
                .ForMember(dest => dest.EquipId, src => src.MapFrom(x => x.ItemId))
                .ForMember(dest => dest.EquipPos, src => src.MapFrom(x => x.Position))
                .ReverseMap()
                .ForMember(dest => dest.ItemId, src => src.MapFrom(x => x.EquipId))
                .ForMember(dest => dest.Position, src => src.MapFrom(x => x.EquipPos));

            CreateMap<PlayerNPCProto.PlayerNPCDto, PlayerNpc>()
                .AfterMap((src, dest) =>
                {
                    dest.setObjectId(dest.Id);
                    dest.setPosition(new Point(dest.X, dest.Cy));
                })
                .ReverseMap();

        }
    }
}
