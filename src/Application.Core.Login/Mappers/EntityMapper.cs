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

            config.NewConfig<AccountEntity, AccountDto.AccountGameDto>()
                .Map(dest => dest.Data, src => AccountDto.AccountGameDataProto.Parser.ParseFrom(src.Blob));
            config.NewConfig<AccountDto.AccountGameDto, AccountEntity>()
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

            config.NewConfig<DropDataGlobalEntity, Dto.DropItemDto>()
                .Map(dest => dest.ItemId, src => src.Itemid)
                .Map(dest => dest.QuestId, src => src.Questid)
                .Map(dest => dest.DropperId, src => src.Continent)
                .Map(dest => dest.Type, _ => DropFromType.GlobalDrop)
                .Map(dest => dest.MinCount, src => src.MinimumQuantity)
                .Map(dest => dest.MaxCount, src => src.MaximumQuantity)
                .Map(dest => dest.Chance, src => src.Chance);

            config.NewConfig<ShopEntity, Dto.ShopDto>();
            config.NewConfig<ShopItemEntity, Dto.ShopItemDto>();

            config.NewConfig<SpecialCashItemEntity, CashProto.SpecialCashItemDto>();

            config.NewConfig<NoteEntity, Dto.NoteDto>();
            config.NewConfig<Dto.NoteDto, NoteEntity>()
                .ConstructUsing(x => new NoteEntity(x.Id, x.ToId, x.FromId, x.Message, x.Timestamp));


            config.NewConfig<RingEntity, RingSourceModel>();
            config.NewConfig<RingSourceModel, RingEntity>();

            config.NewConfig<RingSourceModel, ItemProto.RingDto>();
            config.NewConfig<ItemProto.RingDto, RingSourceModel>();

            #region Gifts
            config.NewConfig<GiftEntity, GiftModel>();
            config.NewConfig<GiftModel, GiftEntity>()
                .ConstructUsing(x => new GiftEntity(x.Id, x.ToId, x.FromId, x.Message, x.Sn, x.RingSourceId));

            config.NewConfig<GiftModel, ItemProto.GiftDto>()
                .Map(dest => dest.To, src => src.ToId)
                .Map(dest => dest.From, src => src.FromId);
            config.NewConfig<ItemProto.GiftDto, GiftModel>()
                .Map(dest => dest.ToId, src => src.To)
                .Map(dest => dest.FromId, src => src.From);
            #endregion

            config.NewConfig<GuildEntity, GuildModel>();
            config.NewConfig<AllianceEntity, AllianceModel>();



            config.NewConfig<FredstorageEntity, FredrickStoreModel>()
                .Map(dest => dest.StoreTime, src => src.Timestamp.ToUnixTimeMilliseconds())
                .Map(dest => dest.Items, src => ItemProto.PlayerShopStoreItems.Parser.ParseFrom(src.ItemsBlob));
            config.NewConfig<FredrickStoreModel, FredstorageEntity>()
                .ConstructUsing(x => new FredstorageEntity(x.Id, x.Cid, x.Daynotes, x.Meso, DateTimeOffset.FromUnixTimeMilliseconds(x.StoreTime)))
                .Map(dest => dest.Timestamp, src => DateTimeOffset.FromUnixTimeMilliseconds(src.StoreTime))
                .Map(dest => dest.ItemsBlob, src => src.Items.ToByteArray());

            config.NewConfig<AccountBindingsEntity, AccountHistoryModel>();
            config.NewConfig<AccountHistoryModel, AccountBindingsEntity>()
                .ConstructUsing(x => new AccountBindingsEntity(x.Id, x.AccountId, x.IP, x.MAC, x.HWID, x.LastActiveTime));

            config.NewConfig<GachaponPoolEntity, ItemProto.GachaponPoolDto>();
            config.NewConfig<GachaponPoolLevelChanceEntity, ItemProto.GachaponPoolChanceDto>();
            config.NewConfig<GachaponPoolItemEntity, ItemProto.GachaponPoolItemDto>();

            config.NewConfig<RewardEntity, CdkCodeModel>();
            config.NewConfig<RewardItemEntity, CdkItemModel>();

            config.NewConfig<RewardRecordEntity, CdkRecordModel>();
            config.NewConfig<CdkRecordModel, RewardRecordEntity>()
                .ConstructUsing(x => new RewardRecordEntity(x.Id, x.CodeId, x.RecipientId, x.RecipientTime));

            config.NewConfig<DueyPackageEntity, DueyDto.DueyPackageDto>()
                .Map(dest => dest.PackageId, src => src.Id)
                .Map(dest => dest.Item, src => src.ItemBlob == null ? null : Dto.ItemDto.Parser.ParseFrom(src.ItemBlob))
                .Map(dest => dest.Notified, src => src.HasNotified);
            config.NewConfig<DueyDto.DueyPackageDto, DueyPackageEntity>()
                .Map(dest => dest.Id, src => src.PackageId)
                .Map(dest => dest.ItemBlob, src => src.Item == null ? null : src.Item.ToByteArray())
                .Map(dest => dest.HasNotified, src => src.Notified);

            config.NewConfig<Dto.NewYearCardDto, NewYearCardEntity>();
            config.NewConfig<NewYearCardEntity, Dto.NewYearCardDto>();

            config.NewConfig<PlifeEntity, LifeProto.PLifeDto>()
                .Map(dest => dest.MapId, src => src.Map)
                .Map(dest => dest.LifeId, src => src.Life);
            config.NewConfig<LifeProto.PLifeDto, PlifeEntity>()
                .ConstructUsing(x => new PlifeEntity(x.Id, x.MapId, x.LifeId, x.Mobtime, x.X, x.Y, x.Fh, x.Type))
                .Map(dest => dest.Map, src => src.MapId)
                .Map(dest => dest.Life, src => src.LifeId);
        }
    }
}
