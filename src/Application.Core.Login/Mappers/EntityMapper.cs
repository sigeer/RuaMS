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
    /// 实体 转 对象（将会被缓存）、或者proto（不会被缓存，直接传输），
    /// 尽量避免通过mapper转实体
    /// </summary>
    public class EntityMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CharacterEntity, CharacterModel>();

            config.NewConfig<AccountEntity, AccountCtrl>();

            config.NewConfig<MonsterbookEntity, MonsterbookModel>();
            config.NewConfig<Trocklocation, TrockLocationModel>();
            config.NewConfig<AreaInfo, AreaModel>();
            config.NewConfig<Eventstat, EventModel>();
            config.NewConfig<FamelogEntity, FameLogModel>()
                .Map(dest => dest.ToId, x => x.CharacteridTo)
                .Map(dest => dest.Time, x => x.When.ToUnixTimeMilliseconds());

            config.NewConfig<QuestStatusEntity, QuestStatusModel>()
                .Map(x => x.QuestId, x => x.Quest)
                .Map(x => x.Id, x => x.Queststatusid);
            config.NewConfig<Questprogress, QuestProgressModel>()
                .Map(dest => dest.ProgressId, x => x.Progressid);
            config.NewConfig<Medalmap, MedalMapModel>();
            config.NewConfig<QuestStatusEntityPair, QuestStatusModel>()
                .Map(dest => dest.MedalMap, x => x.Medalmap)
                .Map(dest => dest.Progress, x => x.Progress);

            config.NewConfig<SkillEntity, SkillModel>();
            config.NewConfig<SkillMacroEntity, SkillMacroModel>();
            config.NewConfig<CooldownEntity, CoolDownModel>();

            config.NewConfig<KeyMapEntity, KeyMapModel>();
            config.NewConfig<Quickslotkeymapped, QuickSlotModel>()
                .Map(dest => dest.LongValue, x => x.Keymap);

            config.NewConfig<SavedLocationEntity, SavedLocationModel>();
            config.NewConfig<StorageEntity, StorageModel>();

            config.NewConfig<PetEntity, PetModel>();


            config.NewConfig<Inventoryequipment, EquipModel>()
                .Map(dest => dest.InventoryItemId, x => x.Inventoryitemid)
                .Map(dest => dest.Id, x => x.Inventoryequipmentid);

            config.NewConfig<Inventoryitem, ItemModel>()
                .Map(dest => dest.InventoryItemId, x => x.Inventoryitemid)
                .Map(dest => dest.InventoryType, x => x.Inventorytype);
            config.NewConfig<ItemEntityPair, ItemModel>()
                .Map(des => des.EquipInfo, x => x.Equip)
                .Map(des => des.PetInfo, x => x.Pet);

            config.NewConfig<ReactorDropEntity, Dto.DropItemDto>()
                .Map(dest => dest.ItemId, x => x.Itemid)
                .Map(dest => dest.QuestId, x => x.Questid)
                .Map(dest => dest.DropperId, x => x.Reactorid)
                .Map(dest => dest.Type, x => DropType.ReactorDrop)
                .Map(dest => dest.MinCount, x => 1)
                .Map(dest => dest.MaxCount, x => 1)
                .Map(dest => dest.Chance, x => x.Chance);

            config.NewConfig<DropDataEntity, Dto.DropItemDto>()
                .Map(dest => dest.ItemId, x => x.Itemid)
                .Map(dest => dest.QuestId, x => x.Questid)
                .Map(dest => dest.DropperId, x => x.Dropperid)
                .Map(dest => dest.Type, x => DropType.MonsterDrop)
                .Map(dest => dest.MinCount, x => x.MinimumQuantity)
                .Map(dest => dest.MaxCount, x => x.MaximumQuantity)
                .Map(dest => dest.Chance, x => x.Chance);

            config.NewConfig<DropDataGlobal, Dto.DropItemDto>()
                .Map(dest => dest.ItemId, x => x.Itemid)
                .Map(dest => dest.QuestId, x => x.Questid)
                .Map(dest => dest.DropperId, x => x.Continent)
                .Map(dest => dest.Type, x => DropType.GlobalDrop)
                .Map(dest => dest.MinCount, x => x.MinimumQuantity)
                .Map(dest => dest.MaxCount, x => x.MaximumQuantity)
                .Map(dest => dest.Chance, x => x.Chance);

            config.NewConfig<NoteEntity, NoteModel>()
                .Map(dest => dest.IsDeleted, x => x.Deleted);
            config.NewConfig<ShopEntity, Dto.ShopDto>();
            config.NewConfig<Shopitem, Dto.ShopItemDto>();

            config.NewConfig<Ring_Entity, RingSourceModel>();

            config.NewConfig<GiftEntity, GiftModel>()
                .Map(dest => dest.To, x => x.ToId)
                .Map(dest => dest.From, x => x.FromId);

            config.NewConfig<SpecialCashItemEntity, CashProto.SpecialCashItemDto>();

            config.NewConfig<GuildEntity, GuildModel>();
            config.NewConfig<AllianceEntity, AllianceModel>();

            config.NewConfig<NewYearCardEntity, NewYearCardModel>().TwoWays();

            config.NewConfig<PlifeEntity, PLifeModel>();

            config.NewConfig<FredstorageEntity, FredrickStoreModel>()
                .Map(dest => dest.UpdateTime, x => x.Timestamp.ToUnixTimeMilliseconds());

            config.NewConfig<AccountBindingsEntity, AccountHistoryModel>();
            config.NewConfig<AccountBanEntity, AccountBanModel>()
                .Map(dest => dest.BanLevel, x => (BanLevel)x.BanLevel);

            config.NewConfig<GachaponPoolEntity, GachaponPoolModel>();
            config.NewConfig<GachaponPoolLevelChanceEntity, GachaponPoolLevelChanceModel>();
            config.NewConfig<GachaponPoolItemEntity, GachaponPoolItemModel>();

            config.NewConfig<CdkCodeEntity, CdkCodeModel>();
            config.NewConfig<CdkItemEntity, CdkItemModel>();
            config.NewConfig<CdkRecordEntity, CdkRecordModel>();


        }
    }
}
