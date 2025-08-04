using Application.Core.EF.Entities;
using Application.Core.EF.Entities.Gachapons;
using Application.Core.EF.Entities.Items;
using Application.Core.EF.Entities.Quests;
using Application.Core.Login.Models;
using Application.Core.Login.Models.Accounts;
using Application.Core.Login.Models.Gachpons;
using Application.Core.Login.Models.Guilds;
using Application.Core.Login.Models.Items;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Items;
using Application.Shared.Login;
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
                .ForMember(dest => dest.Type, src => src.MapFrom(x => DropType.MonsterDrop))
                .ForMember(dest => dest.MinCount, src => src.MapFrom(x => x.MinimumQuantity))
                .ForMember(dest => dest.MaxCount, src => src.MapFrom(x => x.MaximumQuantity))
                .ForMember(dest => dest.Chance, src => src.MapFrom(x => x.Chance));

            CreateMap<DropDataGlobal, Dto.DropItemDto>()
                .ForMember(dest => dest.ItemId, src => src.MapFrom(x => x.Itemid))
                .ForMember(dest => dest.QuestId, src => src.MapFrom(x => x.Questid))
                .ForMember(dest => dest.DropperId, src => src.MapFrom(x => x.Continent))
                .ForMember(dest => dest.Type, src => src.MapFrom(x => DropType.GlobalDrop))
                .ForMember(dest => dest.MinCount, src => src.MapFrom(x => x.MinimumQuantity))
                .ForMember(dest => dest.MaxCount, src => src.MapFrom(x => x.MaximumQuantity))
                .ForMember(dest => dest.Chance, src => src.MapFrom(x => x.Chance));

            CreateMap<NoteEntity, NoteModel>()
                .ForMember(dest => dest.IsDeleted, src => src.MapFrom(x => x.Deleted == 1));
            CreateMap<ShopEntity, Dto.ShopDto>();
            CreateMap<Shopitem, Dto.ShopItemDto>();

            CreateMap<Ring_Entity, RingSourceModel>();

            CreateMap<GiftEntity, GiftModel>()
                .ForMember(dest => dest.To, src => src.MapFrom(x => x.ToId))
                .ForMember(dest => dest.From, src => src.MapFrom(x => x.FromId));

            CreateMap<SpecialCashItemEntity, Dto.SpecialCashItemDto>();

            CreateMap<GuildEntity, GuildModel>();
            CreateMap<AllianceEntity, AllianceModel>();

            CreateMap<NewYearCardEntity, NewYearCardModel>().ReverseMap();

            CreateMap<PlifeEntity, PLifeModel>().ReverseMap();

            CreateMap<FredstorageEntity, FredrickStoreModel>()
                .ForMember(dest => dest.UpdateTime, src => src.MapFrom(x => x.Timestamp.ToUnixTimeMilliseconds()))
                .ReverseMap()
                .ForMember(dest => dest.Timestamp, src => src.MapFrom(x => DateTimeOffset.FromUnixTimeMilliseconds(x.UpdateTime)));

            CreateMap<AccountBindingsEntity, AccountHistoryModel>();
            CreateMap<AccountBanEntity, AccountBanModel>()
                .ForMember(dest => dest.BanLevel, src => src.MapFrom(x => (BanLevel)x.BanLevel));

            CreateMap<GachaponPoolEntity, GachaponPoolModel>();
            CreateMap<GachaponPoolLevelChanceEntity, GachaponPoolLevelChanceModel>();
            CreateMap<GachaponPoolItemEntity, GachaponPoolItemModel>();

        }
    }
}
