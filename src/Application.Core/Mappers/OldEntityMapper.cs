using Application.Core.Game.Items;
using Application.Core.Game.Relation;
using AutoMapper;

namespace Application.Core.Mappers
{
    internal class OldEntityMapper : Profile
    {
        public OldEntityMapper()
        {
            CreateMap<CharacterEntity, Player>()
            .ForMember(x => x.MesoValue, opt => opt.MapFrom(x => new AtomicInteger(x.Meso)))
            .ForMember(x => x.ExpValue, opt => opt.MapFrom(x => new AtomicInteger(x.Exp)))
            .ForMember(x => x.GachaExpValue, opt => opt.MapFrom(x => new AtomicInteger(x.Gachaexp)))
            .ForMember(x => x.RemainingSp, opt => opt.MapFrom(x => TranslateArray(x.Sp)))
            .ForMember(x => x.HP, opt => opt.MapFrom(x => x.Hp))
            .ForMember(x => x.MP, opt => opt.MapFrom(x => x.Mp))
            .ForMember(x => x.MaxHP, opt => opt.MapFrom(x => x.Maxhp))
            .ForMember(x => x.MaxMP, opt => opt.MapFrom(x => x.Maxmp))
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
            .ForMember(x => x.Setupslots, opt => opt.MapFrom(x => x.Bag[InventoryType.SETUP].getSlotLimit()))
            .ForMember(x => x.MountLevel, opt => opt.MapFrom(x => x.MountModel == null ? 1 : x.MountModel.getLevel()))
            .ForMember(x => x.MountExp, opt => opt.MapFrom(x => x.MountModel == null ? 0 : x.MountModel.getExp()))
            .ForMember(x => x.Mounttiredness, opt => opt.MapFrom(x => x.MountModel == null ? 0 : x.MountModel.getTiredness()))
            .ForMember(x => x.Hp, opt => opt.MapFrom(x => x.HP))
            .ForMember(x => x.Mp, opt => opt.MapFrom(x => x.MP))
            .ForMember(x => x.Maxhp, opt => opt.MapFrom(x => x.MaxHP))
            .ForMember(x => x.Maxmp, opt => opt.MapFrom(x => x.MaxMP));

            CreateMap<AllianceEntity, Alliance>()
                .ForMember(x => x.RankTitles, opt => opt.MapFrom(x => new string[] { x.Rank1, x.Rank2, x.Rank3, x.Rank4, x.Rank5 }))
                .ForMember(x => x.AllianceId, opt => opt.MapFrom(x => x.Id))
                .ReverseMap();

            CreateMap<GuildEntity, Guild>().ReverseMap();

            CreateMap<PetEntity, Pet>()
                .ForMember(x => x.Fullness, opt => opt.MapFrom(x => Math.Min(Pet.MaxFullness, x.Fullness)))
                .ForMember(x => x.Level, opt => opt.MapFrom(x => Math.Min(Pet.MaxLevel, x.Level)))
                .ForMember(x => x.Tameness, opt => opt.MapFrom(x => Math.Min(Pet.MaxTameness, x.Closeness)))
                .ReverseMap()
                .ForMember(x => x.Fullness, opt => opt.MapFrom(x => Math.Min(Pet.MaxFullness, x.Fullness)))
                .ForMember(x => x.Level, opt => opt.MapFrom(x => Math.Min(Pet.MaxLevel, (int)x.Level)))
                .ForMember(x => x.Closeness, opt => opt.MapFrom(x => Math.Min(Pet.MaxTameness, x.Tameness)));
        }

        private int[] TranslateArray(string str)
        {
            return str.Split(",").Select(int.Parse).ToArray();
        }
    }
}
