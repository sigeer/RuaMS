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

namespace Application.Core.Login.Mappers
{
    /// <summary>
    /// 实体 转 对象（将会被缓存）、或者proto（不会被缓存，直接传输）
    /// </summary>
    public class EntityMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CharacterEntity, CharacterModel>();
            config.NewConfig<CharacterModel, CharacterEntity>();
            config.NewConfig<Dto.CharacterDto, CharacterEntity>();

            config.NewConfig<AccountEntity, AccountCtrl>();

            config.NewConfig<MonsterbookEntity, MonsterbookModel>();
            config.NewConfig<Trocklocation, TrockLocationModel>();
            config.NewConfig<AreaInfo, AreaModel>();
            config.NewConfig<Eventstat, EventModel>();
            config.NewConfig<FamelogEntity, FameLogModel>()
                .Map(dest => dest.ToId, src => src.CharacteridTo)
                .Map(dest => dest.Time, src => src.When.ToUnixTimeMilliseconds());

            config.NewConfig<QuestStatusEntity, QuestStatusModel>()
                .Map(dest => dest.QuestId, src => src.Quest)
                .Map(dest => dest.Id, src => src.Queststatusid);
            config.NewConfig<Questprogress, QuestProgressModel>()
                .Map(dest => dest.ProgressId, src => src.Progressid);
            config.NewConfig<Medalmap, MedalMapModel>();
            config.NewConfig<QuestStatusEntityPair, QuestStatusModel>()
                .Map(dest => dest.MedalMap, src => src.Medalmap)
                .Map(dest => dest.Progress, src => src.Progress)
                .Map(dest => dest.QuestId, src => src.QuestStatus.Quest)
                .Map(dest => dest.Id, src => src.QuestStatus.Queststatusid);

            config.NewConfig<SkillEntity, SkillModel>();
            config.NewConfig<SkillMacroEntity, SkillMacroModel>();
            config.NewConfig<CooldownEntity, CoolDownModel>();

            config.NewConfig<KeyMapEntity, KeyMapModel>();
            config.NewConfig<Quickslotkeymapped, QuickSlotModel>()
                .Map(dest => dest.LongValue, src => src.Keymap);

            config.NewConfig<SavedLocationEntity, SavedLocationModel>();
            config.NewConfig<StorageEntity, StorageModel>();

            config.NewConfig<PetEntity, PetModel>();


            config.NewConfig<Inventoryequipment, EquipModel>()
                .Map(dest => dest.Id, src => src.Inventoryequipmentid);

            config.NewConfig<Inventoryitem, ItemModel>()
                .Map(dest => dest.InventoryType, src => src.Inventorytype);
            config.NewConfig<ItemEntityPair, ItemModel>()
                .Map(dest => dest.EquipInfo, src => src.Equip)
                .Map(dest => dest.PetInfo, src => src.Pet)
                .Map(dest => dest.InventoryType, src => src.Item.Inventorytype);

            config.NewConfig<ReactorDropEntity, Dto.DropItemDto>()
                .Map(dest => dest.ItemId, src => src.Itemid)
                .Map(dest => dest.QuestId, src => src.Questid)
                .Map(dest => dest.DropperId, src => src.Reactorid)
                .Map(dest => dest.Type, _ => DropFromType.ReactorDrop)
                .Map(dest => dest.MinCount, _ => 1)
                .Map(dest => dest.MaxCount, _ => 1)
                .Map(dest => dest.Chance, src => src.Chance);

            config.NewConfig<DropDataEntity, Dto.DropItemDto>()
                .Map(dest => dest.ItemId, src => src.Itemid)
                .Map(dest => dest.QuestId, src => src.Questid)
                .Map(dest => dest.DropperId, src => src.Dropperid)
                .Map(dest => dest.Type, _ => DropFromType.MonsterDrop)
                .Map(dest => dest.MinCount, src => src.MinimumQuantity)
                .Map(dest => dest.MaxCount, src => src.MaximumQuantity)
                .Map(dest => dest.Chance, src => src.Chance);

            config.NewConfig<DropDataGlobal, Dto.DropItemDto>()
                .Map(dest => dest.ItemId, src => src.Itemid)
                .Map(dest => dest.QuestId, src => src.Questid)
                .Map(dest => dest.DropperId, src => src.Continent)
                .Map(dest => dest.Type, _ => DropFromType.GlobalDrop)
                .Map(dest => dest.MinCount, src => src.MinimumQuantity)
                .Map(dest => dest.MaxCount, src => src.MaximumQuantity)
                .Map(dest => dest.Chance, src => src.Chance);

            config.NewConfig<NoteEntity, NoteModel>()
                .Map(dest => dest.IsDeleted, src => src.Deleted);
            config.NewConfig<ShopEntity, Dto.ShopDto>();
            config.NewConfig<Shopitem, Dto.ShopItemDto>();

            config.NewConfig<Ring_Entity, RingSourceModel>();

            config.NewConfig<GiftEntity, GiftModel>()
                .Map(dest => dest.To, src => src.ToId)
                .Map(dest => dest.From, src => src.FromId);

            config.NewConfig<SpecialCashItemEntity, CashProto.SpecialCashItemDto>();

            config.NewConfig<GuildEntity, GuildModel>();
            config.NewConfig<AllianceEntity, AllianceModel>();

            config.NewConfig<NewYearCardEntity, NewYearCardModel>();
            config.NewConfig<NewYearCardModel, NewYearCardEntity>();

            config.NewConfig<PlifeEntity, PLifeModel>();
            config.NewConfig<PLifeModel, PlifeEntity>();

            config.NewConfig<FredstorageEntity, FredrickStoreModel>()
                .Map(dest => dest.StoreTime, src => src.Timestamp.ToUnixTimeMilliseconds());
            config.NewConfig<FredrickStoreModel, FredstorageEntity>()
                .Map(dest => dest.Timestamp, src => DateTimeOffset.FromUnixTimeMilliseconds(src.StoreTime));

            config.NewConfig<AccountBindingsEntity, AccountHistoryModel>();
            config.NewConfig<AccountBanEntity, AccountBanModel>()
                .Map(dest => dest.BanLevel, src => (BanLevel)src.BanLevel);

            config.NewConfig<GachaponPoolEntity, GachaponPoolModel>();
            config.NewConfig<GachaponPoolLevelChanceEntity, GachaponPoolLevelChanceModel>();
            config.NewConfig<GachaponPoolItemEntity, GachaponPoolItemModel>();

            config.NewConfig<CdkCodeEntity, CdkCodeModel>();
            config.NewConfig<CdkItemEntity, CdkItemModel>();
            config.NewConfig<CdkRecordEntity, CdkRecordModel>();

        }
    }
}
