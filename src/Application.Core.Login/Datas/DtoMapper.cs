using Application.Core.EF.Entities.Items;
using Application.Core.Game.Players;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Characters;
using Application.Shared.Items;
using AutoMapper;
using tools;

namespace Application.Core.Login.Datas
{
    /// <summary>
    /// 实体与DTO
    /// </summary>
    public class DtoMapper : Profile
    {
        public DtoMapper()
        {
            CreateMap<CharacterEntity, CharacterDto>()
                .ReverseMap();

            CreateMap<AccountEntity, AccountDto>().ReverseMap();

            CreateMap<Trocklocation, TrockLocationDto>()
                .ReverseMap();
            CreateMap<AreaInfo, AreaDto>().ReverseMap();
            CreateMap<Eventstat, EventDto>().ReverseMap();

            CreateMap<QuestStatusEntity, QuestStatusDto>().ReverseMap();
            CreateMap<Questprogress, QuestProgressDto>().ReverseMap();
            CreateMap<Medalmap, MedalMapDto>().ReverseMap();

            CreateMap<SkillEntity, SkillDto>().ReverseMap();
            CreateMap<SkillMacroEntity, SkillMacroDto>().ReverseMap();
            CreateMap<CooldownEntity, CoolDownDto>().ReverseMap();

            CreateMap<KeyMapEntity, KeyMapDto>().ReverseMap();
            CreateMap<Quickslotkeymapped, QuickSlotDto>()
                .ForMember(dest => dest.Value, source => source.MapFrom(x => x.Keymap))
                .ForMember(dest => dest.QuickSlotLoaded, source => source.MapFrom(x => LongTool.LongToBytes(x.Keymap)))
                .ReverseMap();

            CreateMap<SavedLocationEntity, SavedLocationDto>().ReverseMap();
            CreateMap<StorageEntity, StorageDto>().ReverseMap();

            CreateMap<Inventoryitem, ItemDto>();
            CreateMap<Inventoryequipment, EquipDto>();
            CreateMap<PetEntity, PetDto>();
            CreateMap<ItemEntityPair, ItemDto>()
                .ForMember(des => des.EquipInfo, source => source.MapFrom(x => x.Equip))
                .ForMember(des =>des.PetInfo, source => source.MapFrom(x => x.Pet))
                .IncludeMembers(source => source.Item);
        }
    }
}
