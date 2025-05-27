using Application.Core.EF.Entities.SystemBase;
using Application.Core.Managers;
using Application.EF.Entities;
using Application.Utility.Configs;
using AutoMapper;
using server;
using server.life;
using server.quest;

namespace Application.Host.Models
{
    public class DtoMapper : Profile
    {
        public DtoMapper()
        {
            CreateMap(typeof(PagedData<>), typeof(PagedData<>));

            CreateMap<DropDataEntity, DropDataDto>().ForMember(a => a.QuestId, b => b.MapFrom(x => x.Questid))
                .ForMember(a => a.ItemId, b => b.MapFrom(x => x.Itemid))
                .ForMember(a => a.DropperId, b => b.MapFrom(x => x.Dropperid))
                .ForMember(a => a.ItemName, b => b.MapFrom(x => ItemInformationProvider.getInstance().getName(x.Itemid)))
                .ForMember(a => a.DropperName, b => b.MapFrom(x => MonsterInformationProvider.getInstance().getMobNameFromId(x.Dropperid)))
                .ForMember(a => a.QuestName, b => b.MapFrom(x => Quest.getInstance(x.Questid).getName()))
                .ReverseMap();

            CreateMap<DropDataGlobal, DropDataDto>().ForMember(a => a.QuestId, b => b.MapFrom(x => x.Questid))
                .ForMember(a => a.ItemId, b => b.MapFrom(x => x.Itemid))
                .ForMember(a => a.ContinentId, b => b.MapFrom(x => x.Continent))
                .ForMember(a => a.ItemName, b => b.MapFrom(x => ItemInformationProvider.getInstance().getName(x.Itemid)))
                .ForMember(a => a.ContinentName, b => b.MapFrom(x => x.Continent < 0 ? "-" : GetContinentName(x.Continent)))
                .ForMember(a => a.QuestName, b => b.MapFrom(x => Quest.getInstance(x.Questid).getName()))
                .ReverseMap();

            CreateMap<WorldServerConfig, WorldConfigEntity>().ReverseMap();
            CreateMap<WorldServerDto, WorldConfigEntity>().ReverseMap();
        }

        private string GetContinentName(int id)
        {
            if (id == -1)
                return "全部";

            return ServerManager.GetWorldName(id) ?? $"未知 (Id: {id}) ";
        }
    }
}
