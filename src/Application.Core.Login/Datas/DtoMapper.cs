using Application.Core.EF.Entities.Items;
using Application.Core.EF.Entities.Quests;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Characters;
using Application.Shared.Items;
using AutoMapper;

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

            CreateMap<AccountEntity, AccountDto>();
            CreateMap<MonsterbookEntity, MonsterbookDto>();
            CreateMap<Trocklocation, TrockLocationDto>();
            CreateMap<AreaInfo, AreaDto>();
            CreateMap<Eventstat, EventDto>();

            CreateMap<QuestStatusEntity, QuestStatusDto>()
                .ForMember(x => x.QuestId, src => src.MapFrom(x => x.Quest))
                .ForMember(x => x.Id, src => src.MapFrom(x => x.Queststatusid));
            CreateMap<Questprogress, QuestProgressDto>()
                .ForMember(dest => dest.ProgressId, source => source.MapFrom(x => x.Progressid));
            CreateMap<Medalmap, MedalMapDto>();
            CreateMap<QuestStatusEntityPair, QuestStatusDto>()
                .ForMember(dest => dest.MedalMap, source => source.MapFrom(x => x.Medalmap))
                .ForMember(dest => dest.Progress, source => source.MapFrom(x => x.Progress))
                .IncludeMembers(source => source.QuestStatus);

            CreateMap<SkillEntity, SkillDto>();
            CreateMap<SkillMacroEntity, SkillMacroDto>();
            CreateMap<CooldownEntity, CoolDownDto>();

            CreateMap<KeyMapEntity, KeyMapDto>();
            CreateMap<Quickslotkeymapped, QuickSlotDto>()
                .ForMember(dest => dest.LongValue, source => source.MapFrom(x => x.Keymap));

            CreateMap<SavedLocationEntity, SavedLocationDto>();
            CreateMap<StorageEntity, StorageDto>();

            CreateMap<PetEntity, PetDto>();
            CreateMap<Ring_Entity, RingDto>();

            CreateMap<Inventoryequipment, EquipDto>()
                .ForMember(dest => dest.InventoryItemId, source => source.MapFrom(x => x.Inventoryitemid))
                .ForMember(dest => dest.Id, source => source.MapFrom(x => x.Inventoryequipmentid))
                .ReverseMap()
                .ForMember(dest => dest.Inventoryitemid, source => source.MapFrom(x => x.InventoryItemId));

            CreateMap<EquipEntityPair, EquipDto>()
                .ForMember(dest => dest.RingInfo, source => source.MapFrom(x => x.Ring))
                .IncludeMembers(source => source.Equip)
                .ReverseMap()
                .ForMember(dest => dest.Ring, source => source.MapFrom(x => x.RingInfo))
                .ForMember(dest => dest.Equip, source => source.MapFrom(x => x));

            CreateMap<Inventoryitem, ItemDto>()
                .ForMember(dest => dest.InventoryItemId, source => source.MapFrom(x => x.Inventoryitemid))
                .ForMember(dest => dest.InventoryType, source => source.MapFrom(x => x.Inventorytype));
            CreateMap<ItemEntityPair, ItemDto>()
                .ForMember(des => des.EquipInfo, source => source.MapFrom(x => x.Equip))
                .ForMember(des => des.PetInfo, source => source.MapFrom(x => x.Pet))
                .IncludeMembers(source => source.Item);

            CreateMap<ReactorDropEntity, DropDto>()
                .ForMember(dest => dest.ItemId, src => src.MapFrom(x => x.Itemid))
                .ForMember(dest => dest.QuestId, src => src.MapFrom(x => x.Questid))
                .ForMember(dest => dest.DropperId, src => src.MapFrom(x => x.Reactorid))
                .ForMember(dest => dest.Type, src => src.MapFrom(x => DropType.ReactorDrop))
                .ForMember(dest => dest.MinCount, src => src.MapFrom(x => 1))
                .ForMember(dest => dest.MaxCount, src => src.MapFrom(x => 1))
                .ForMember(dest => dest.Chance, src => src.MapFrom(x => x.Chance));

            CreateMap<DropDataEntity, DropDto>()
                .ForMember(dest => dest.ItemId, src => src.MapFrom(x => x.Itemid))
                .ForMember(dest => dest.QuestId, src => src.MapFrom(x => x.Questid))
                .ForMember(dest => dest.DropperId, src => src.MapFrom(x => x.Dropperid))
                .ForMember(dest => dest.Type, src => src.MapFrom(x => DropType.ReactorDrop))
                .ForMember(dest => dest.MinCount, src => src.MapFrom(x => x.MinimumQuantity))
                .ForMember(dest => dest.MaxCount, src => src.MapFrom(x => x.MaximumQuantity))
                .ForMember(dest => dest.Chance, src => src.MapFrom(x => x.Chance));

            CreateMap<GiftEntity, GiftDto>()
                .ForMember(dest => dest.RingId, src => src.MapFrom(x => x.Ringid));
        }
    }
}
