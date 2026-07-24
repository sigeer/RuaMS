using Application.Core.EF.Entities;
using Application.Core.EF.Entities.Gachapons;
using Application.Core.Login.Models;
using Application.Core.Login.Models.Accounts;
using Application.Core.Login.Models.Gachpons;
using Application.Core.Login.Models.Guilds;
using Application.Core.Login.Models.Items;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Items;
using Application.Shared.Login;
using Google.Protobuf;
using Microsoft.EntityFrameworkCore.Design;

namespace Application.Core.Login.Mappers
{
    /// <summary>
    /// 实体 转 对象（将会被缓存）、或者proto（不会被缓存，直接传输）
    /// </summary>
    public class EntityMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CharacterEntity, Dto.CharacterDto>()
                .Map(dest => dest.Data, src => Dto.CharacterDataProto.Parser.ParseFrom(src.Blob));

            config.NewConfig<Dto.CharacterDto, CharacterEntity>()
                .Map(dest => dest.Blob, src => src.Data.ToByteArray());

            config.NewConfig<AccountEntity, Dto.AccountGameDto>()
                .Map(dest => dest.Data, src => Dto.AccountGameDataProto.Parser.ParseFrom(src.Blob));
            config.NewConfig<Dto.AccountGameDto, AccountEntity>()
                .Map(dest => dest.Blob, src => src.Data.ToByteArray());


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

            config.NewConfig<RingEntity, RingSourceModel>();

            config.NewConfig<GiftEntity, GiftModel>()
                .Map(dest => dest.To, src => src.ToId)
                .Map(dest => dest.From, src => src.FromId);

            config.NewConfig<SpecialCashItemEntity, CashProto.SpecialCashItemDto>();

            config.NewConfig<GuildEntity, GuildModel>();
            config.NewConfig<AllianceEntity, AllianceModel>();

            config.NewConfig<PlifeEntity, PLifeModel>();
            config.NewConfig<PLifeModel, PlifeEntity>();

            config.NewConfig<FredstorageEntity, FredrickStoreModel>()
                .Map(dest => dest.StoreTime, src => src.Timestamp.ToUnixTimeMilliseconds())
                .Map(dest => dest.Items, src => ItemProto.PlayerShopStoreItems.Parser.ParseFrom(src.ItemsBlob));
            config.NewConfig<FredrickStoreModel, FredstorageEntity>()
                .Map(dest => dest.Timestamp, src => DateTimeOffset.FromUnixTimeMilliseconds(src.StoreTime))
                .Map(dest => dest.ItemsBlob, src => src.Items.ToByteArray());

            config.NewConfig<AccountBindingsEntity, AccountHistoryModel>();
            config.NewConfig<AccountBanEntity, AccountBanModel>()
                .Map(dest => dest.BanLevel, src => (BanLevel)src.BanLevel);

            config.NewConfig<GachaponPoolEntity, GachaponPoolModel>();
            config.NewConfig<GachaponPoolLevelChanceEntity, GachaponPoolLevelChanceModel>();
            config.NewConfig<GachaponPoolItemEntity, GachaponPoolItemModel>();

            config.NewConfig<CdkCodeEntity, CdkCodeModel>();
            config.NewConfig<CdkItemEntity, CdkItemModel>();
            config.NewConfig<CdkRecordEntity, CdkRecordModel>();

            config.NewConfig<DueyPackageEntity, DueyDto.DueyPackageDto>()
                .Map(dest => dest.Item, src => src.ItemBlob == null ? null : Dto.ItemDto.Parser.ParseFrom(src.ItemBlob))
                .Map(dest => dest.Notified, src => src.HasNotified);
            config.NewConfig<DueyDto.DueyPackageDto, DueyPackageEntity>()
                .Map(dest => dest.ItemBlob, src => src.Item == null ? null : src.Item.ToByteArray())
                .Map(dest => dest.HasNotified, src => src.Notified);
        }
    }
}
