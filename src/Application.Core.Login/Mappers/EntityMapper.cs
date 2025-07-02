using Application.Core.EF.Entities.Items;
using Application.Core.EF.Entities.Quests;
using Application.Core.Login.Models;
using Application.Core.Login.Models.Guilds;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Items;
using Application.Shared.NewYear;
using AutoMapper;

namespace Application.Core.Login.Mappers
{
    /// <summary>
    /// 实体 转 对象（将会被缓存）、或者proto（不会被缓存，直接传输）
    /// </summary>
    public class EntityMapper : Profile
    {
        public EntityMapper()
        {
            CreateMap<CharacterEntity, CharacterModel>()
                .ReverseMap();
            CreateMap<Dto.CharacterDto, CharacterEntity>();

            CreateMap<AccountEntity, AccountCtrl>();

            CreateMap<MonsterbookEntity, MonsterbookModel>();
            CreateMap<Trocklocation, TrockLocationModel>();
            CreateMap<AreaInfo, AreaModel>();
            CreateMap<Eventstat, EventModel>();

            CreateMap<QuestStatusEntity, QuestStatusModel>()
                .ForMember(x => x.QuestId, src => src.MapFrom(x => x.Quest))
                .ForMember(x => x.Id, src => src.MapFrom(x => x.Queststatusid));
            CreateMap<Questprogress, QuestProgressModel>()
                .ForMember(dest => dest.ProgressId, source => source.MapFrom(x => x.Progressid));
            CreateMap<Medalmap, MedalMapModel>();
            CreateMap<QuestStatusEntityPair, QuestStatusModel>()
                .ForMember(dest => dest.MedalMap, source => source.MapFrom(x => x.Medalmap))
                .ForMember(dest => dest.Progress, source => source.MapFrom(x => x.Progress))
                .IncludeMembers(source => source.QuestStatus);

            CreateMap<SkillEntity, SkillModel>();
            CreateMap<SkillMacroEntity, SkillMacroModel>();
            CreateMap<CooldownEntity, CoolDownModel>();

            CreateMap<KeyMapEntity, KeyMapModel>();
            CreateMap<Quickslotkeymapped, QuickSlotModel>()
                .ForMember(dest => dest.LongValue, source => source.MapFrom(x => x.Keymap));

            CreateMap<SavedLocationEntity, SavedLocationModel>();
            CreateMap<StorageEntity, StorageModel>();

            CreateMap<PetEntity, PetModel>();


            CreateMap<Inventoryequipment, EquipModel>()
                .ForMember(dest => dest.InventoryItemId, source => source.MapFrom(x => x.Inventoryitemid))
                .ForMember(dest => dest.Id, source => source.MapFrom(x => x.Inventoryequipmentid));

            CreateMap<EquipEntityPair, EquipModel>()
                .ForMember(dest => dest.RingInfo, source => source.MapFrom(x => x.Ring))
                .IncludeMembers(source => source.Equip)
                .ReverseMap()
                .ForMember(dest => dest.Ring, source => source.MapFrom(x => x.RingInfo))
                .ForMember(dest => dest.Equip, source => source.MapFrom(x => x));

            CreateMap<Inventoryitem, ItemModel>()
                .ForMember(dest => dest.InventoryItemId, source => source.MapFrom(x => x.Inventoryitemid))
                .ForMember(dest => dest.InventoryType, source => source.MapFrom(x => x.Inventorytype));
            CreateMap<ItemEntityPair, ItemModel>()
                .ForMember(des => des.EquipInfo, source => source.MapFrom(x => x.Equip))
                .ForMember(des => des.PetInfo, source => source.MapFrom(x => x.Pet))
            .IncludeMembers(source => source.Item);

            CreateMap<ReactorDropEntity, Dto.DropItemDto>()
                .ForMember(dest => dest.ItemId, src => src.MapFrom(x => x.Itemid))
                .ForMember(dest => dest.QuestId, src => src.MapFrom(x => x.Questid))
                .ForMember(dest => dest.DropperId, src => src.MapFrom(x => x.Reactorid))
                .ForMember(dest => dest.Type, src => src.MapFrom(x => DropType.ReactorDrop))
                .ForMember(dest => dest.MinCount, src => src.MapFrom(x => 1))
                .ForMember(dest => dest.MaxCount, src => src.MapFrom(x => 1))
                .ForMember(dest => dest.Chance, src => src.MapFrom(x => x.Chance));

            CreateMap<DropDataEntity, Dto.DropItemDto>()
                .ForMember(dest => dest.ItemId, src => src.MapFrom(x => x.Itemid))
                .ForMember(dest => dest.QuestId, src => src.MapFrom(x => x.Questid))
                .ForMember(dest => dest.DropperId, src => src.MapFrom(x => x.Dropperid))
                .ForMember(dest => dest.Type, src => src.MapFrom(x => DropType.ReactorDrop))
                .ForMember(dest => dest.MinCount, src => src.MapFrom(x => x.MinimumQuantity))
                .ForMember(dest => dest.MaxCount, src => src.MapFrom(x => x.MaximumQuantity))
                .ForMember(dest => dest.Chance, src => src.MapFrom(x => x.Chance));

            CreateMap<NoteEntity, Dto.NoteDto>();
            CreateMap<ShopEntity, Dto.ShopDto>();
            CreateMap<Shopitem, Dto.ShopItemDto>();

            CreateMap<Ring_Entity, RingModel>();

            CreateMap<GiftRingPair, Dto.GiftDto>()
                .ForMember(x => x.Ring, src => src.MapFrom(x => x.Ring))
                .IncludeMembers(x => x.Gift);
            CreateMap<GiftEntity, Dto.GiftDto>();
            CreateMap<Ring_Entity, Dto.RingDto>();
            CreateMap<SpecialCashItemEntity, Dto.SpecialCashItemDto>();

            CreateMap<GuildEntity, GuildModel>();
            CreateMap<AllianceEntity, AllianceModel>();

            CreateMap<NewYearCardEntity, NewYearCardModel>();
        }
    }
}
