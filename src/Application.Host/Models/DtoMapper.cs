using Application.EF.Entities;
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

            CreateMap<DropDatum, DropDataDto>().ForMember(a => a.QuestId, b => b.MapFrom(x => x.Questid))
                .ForMember(a => a.ItemId, b => b.MapFrom(x => x.Itemid))
                .ForMember(a => a.DropperId, b => b.MapFrom(x => x.Dropperid))
                .ForMember(a => a.ItemName, b => b.MapFrom(x => ItemInformationProvider.getInstance().getName(x.Itemid)))
                .ForMember(a => a.DropperName, b => b.MapFrom(x => MonsterInformationProvider.getInstance().getMobNameFromId(x.Dropperid)))
                .ForMember(a => a.QuestName, b => b.MapFrom(x => Quest.getInstance(x.Questid).getName()))
                .ReverseMap();
        }
    }
}
