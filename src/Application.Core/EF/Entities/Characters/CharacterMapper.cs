using AutoMapper;
using client.inventory;

namespace Application.Core.EF.Entities
{
    public class CharacterMapper : Profile
    {
        public CharacterMapper()
        {
            CreateMap<CharacterEntity, Player>()
                .ForMember(x => x.MesoValue, opt => opt.MapFrom(x => new AtomicInteger(x.Meso)))
                .ForMember(x => x.ExpValue, opt => opt.MapFrom(x => new AtomicLong(x.Exp)))
                .ForMember(x => x.Gachaexp, opt => opt.MapFrom(x => new AtomicLong(x.Gachaexp)))
                .ForMember(x => x.RemainingSp, opt => opt.MapFrom(x => TranslateArray(x.Sp)))
                .AfterMap((before, after) => after.Reload())
                .ReverseMap()
                .ForMember(x => x.Sp, opt => opt.MapFrom(x => string.Join(",", x.RemainingSp)))
                .ForMember(x => x.Meso, opt => opt.MapFrom(x => x.MesoValue.get()))
                .ForMember(x => x.Exp, opt => opt.MapFrom(x => x.ExpValue.get()))
                .ForMember(x => x.Gachaexp, opt => opt.MapFrom(x => x.GachaExpValue.get()))
                .ForMember(x => x.BuddyCapacity, opt => opt.MapFrom(x => x.BuddyList.Capacity))
                .ForMember(x => x.Equipslots, opt => opt.MapFrom(x => x.Bag[InventoryType.EQUIP].getSlotLimit()))
                .ForMember(x => x.Useslots, opt => opt.MapFrom(x => x.Bag[InventoryType.USE].getSlotLimit()))
                .ForMember(x => x.Etcslots, opt => opt.MapFrom(x => x.Bag[InventoryType.ETC].getSlotLimit()))
                .ForMember(x => x.Setupslots, opt => opt.MapFrom(x => x.Bag[InventoryType.SETUP].getSlotLimit()));
        }

        private int[] TranslateArray(string str)
        {
            return str.Split(",").Select(int.Parse).ToArray();
        }
    }
}
