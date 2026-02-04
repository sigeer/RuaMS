using Application.Core.Login.Models;
using Application.EF.Entities;
using Application.Templates.Item.Consume;
using AutoMapper;

namespace Application.Core.Login.Mappers
{
    internal class PlayerNPCMapper : Profile
    {
        public PlayerNPCMapper()
        {
            CreateMap<PlayerNpcEquipModel, LifeProto.PlayerNPCEquip>()
                .ForMember(dest => dest.ItemId, src => src.MapFrom(x => x.Equipid))
                .ForMember(dest => dest.Position, src => src.MapFrom(x => x.Equippos))
                .ReverseMap()
                .ForMember(dest => dest.Equipid, src => src.MapFrom(x => x.ItemId))
                .ForMember(dest => dest.Equippos, src => src.MapFrom(x => x.Position));
            CreateMap<PlayerNpcModel, LifeProto.PlayerNPCDto>()
                .ForMember(dest => dest.MapId, src => src.MapFrom(x => x.Map))
                .ForMember(dest => dest.ScriptId, src => src.MapFrom(x => x.NpcId))
                .ReverseMap()
                .ForMember(dest => dest.Map, src => src.MapFrom(x => x.MapId))
                .ForMember(dest => dest.NpcId, src => src.MapFrom(x => x.ScriptId));

            CreateMap<PlayerNpcEntity, PlayerNpcModel>()
                .ForMember(dest => dest.NpcId, src => src.MapFrom(x => x.Scriptid))
                .ReverseMap()
                .ForMember(dest => dest.Scriptid, src => src.MapFrom(x => x.NpcId));
            CreateMap<PlayerNpcsEquipEntity, PlayerNpcEquipModel>()
                .ReverseMap();
        }
    }
}
