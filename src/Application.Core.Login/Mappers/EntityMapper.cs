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
            config.NewConfig<CharacterEntity, ProtoModel.CharacterProto>()
                .Map(dest => dest.Data, src => ProtoModel.CharacterDataProto.Parser.ParseFrom(src.Blob));

            config.NewConfig<ProtoModel.CharacterProto, CharacterEntity>()
                .Map(dest => dest.Blob, src => src.Data.ToByteArray());

            config.NewConfig<AccountEntity, ProtoModel.AccountGameProto>()
                .Map(dest => dest.Data, src => ProtoModel.AccountGameDataProto.Parser.ParseFrom(src.Blob));
            config.NewConfig<ProtoModel.AccountGameProto, AccountEntity>()
                .Map(dest => dest.Blob, src => src.Data.ToByteArray());


            config.NewConfig<ReactorDropEntity, ProtoModel.DropItemProto>()
                .Map(dest => dest.ItemId, src => src.Itemid)
                .Map(dest => dest.QuestId, src => src.Questid)
                .Map(dest => dest.DropperId, src => src.Reactorid)
                .Map(dest => dest.Type, _ => DropFromType.ReactorDrop)
                .Map(dest => dest.MinCount, _ => 1)
                .Map(dest => dest.MaxCount, _ => 1)
                .Map(dest => dest.Chance, src => src.Chance);

            config.NewConfig<DropDataEntity, ProtoModel.DropItemProto>()
                .Map(dest => dest.ItemId, src => src.Itemid)
                .Map(dest => dest.QuestId, src => src.Questid)
                .Map(dest => dest.DropperId, src => src.Dropperid)
                .Map(dest => dest.Type, _ => DropFromType.MonsterDrop)
                .Map(dest => dest.MinCount, src => src.MinimumQuantity)
                .Map(dest => dest.MaxCount, src => src.MaximumQuantity)
                .Map(dest => dest.Chance, src => src.Chance);

            config.NewConfig<DropDataGlobalEntity, ProtoModel.DropItemProto>()
                .Map(dest => dest.ItemId, src => src.Itemid)
                .Map(dest => dest.QuestId, src => src.Questid)
                .Map(dest => dest.DropperId, src => src.Continent)
                .Map(dest => dest.Type, _ => DropFromType.GlobalDrop)
                .Map(dest => dest.MinCount, src => src.MinimumQuantity)
                .Map(dest => dest.MaxCount, src => src.MaximumQuantity)
                .Map(dest => dest.Chance, src => src.Chance);

            config.NewConfig<ShopEntity, ProtoModel.ShopProto>();
            config.NewConfig<ShopItemEntity, ProtoModel.ShopItemProto>();

            config.NewConfig<SpecialCashItemEntity, ProtoModel.SpecialCashItemProto>();

            config.NewConfig<NoteEntity, ProtoModel.NoteProto>();
            config.NewConfig<ProtoModel.NoteProto, NoteEntity>()
                .ConstructUsing(x => new NoteEntity(x.Id, x.ToId, x.FromId, x.Message, x.Timestamp));


            config.NewConfig<RingEntity, RingSourceModel>();
            config.NewConfig<RingSourceModel, RingEntity>();

            config.NewConfig<RingSourceModel, ProtoModel.RingProto>();
            config.NewConfig<ProtoModel.RingProto, RingSourceModel>();

            #region Gifts
            config.NewConfig<GiftEntity, GiftModel>();
            config.NewConfig<GiftModel, GiftEntity>()
                .ConstructUsing(x => new GiftEntity(x.Id, x.ToId, x.FromId, x.Message, x.Sn, x.RingSourceId));

            config.NewConfig<GiftModel, ProtoModel.GiftProto>()
                .Map(dest => dest.To, src => src.ToId)
                .Map(dest => dest.From, src => src.FromId);
            config.NewConfig<ProtoModel.GiftProto, GiftModel>()
                .Map(dest => dest.ToId, src => src.To)
                .Map(dest => dest.FromId, src => src.From);
            #endregion

            config.NewConfig<GuildEntity, GuildModel>();
            config.NewConfig<AllianceEntity, AllianceModel>();



            config.NewConfig<FredstorageEntity, FredrickStoreModel>()
                .Map(dest => dest.StoreTime, src => src.Timestamp.ToUnixTimeMilliseconds())
                .Map(dest => dest.Items, src => ProtoModel.PlayerShopStoreItemsProto.Parser.ParseFrom(src.ItemsBlob));
            config.NewConfig<FredrickStoreModel, FredstorageEntity>()
                .ConstructUsing(x => new FredstorageEntity(x.Id, x.Cid, x.Daynotes, x.Meso, DateTimeOffset.FromUnixTimeMilliseconds(x.StoreTime)))
                .Map(dest => dest.Timestamp, src => DateTimeOffset.FromUnixTimeMilliseconds(src.StoreTime))
                .Map(dest => dest.ItemsBlob, src => src.Items.ToByteArray());

            config.NewConfig<AccountBindingsEntity, AccountHistoryModel>();
            config.NewConfig<AccountHistoryModel, AccountBindingsEntity>()
                .ConstructUsing(x => new AccountBindingsEntity(x.Id, x.AccountId, x.IP, x.MAC, x.HWID, x.LastActiveTime));

            config.NewConfig<GachaponPoolEntity, ProtoModel.GachaponPoolProto>();
            config.NewConfig<GachaponPoolLevelChanceEntity, ProtoModel.GachaponPoolChanceProto>();
            config.NewConfig<GachaponPoolItemEntity, ProtoModel.GachaponPoolItemProto>();

            config.NewConfig<RewardEntity, CdkCodeModel>();
            config.NewConfig<RewardItemEntity, CdkItemModel>();

            config.NewConfig<RewardRecordEntity, CdkRecordModel>();
            config.NewConfig<CdkRecordModel, RewardRecordEntity>()
                .ConstructUsing(x => new RewardRecordEntity(x.Id, x.CodeId, x.RecipientId, x.RecipientTime));

            config.NewConfig<DueyPackageEntity, ProtoModel.DueyPackageProto>()
                .Map(dest => dest.PackageId, src => src.Id)
                .Map(dest => dest.Item, src => src.ItemBlob == null ? null : ProtoModel.ItemProto.Parser.ParseFrom(src.ItemBlob))
                .Map(dest => dest.Notified, src => src.HasNotified);
            config.NewConfig<ProtoModel.DueyPackageProto, DueyPackageEntity>()
                .Map(dest => dest.Id, src => src.PackageId)
                .Map(dest => dest.ItemBlob, src => src.Item == null ? null : src.Item.ToByteArray())
                .Map(dest => dest.HasNotified, src => src.Notified);

            config.NewConfig<ProtoModel.NewYearCardProto, NewYearCardEntity>();
            config.NewConfig<NewYearCardEntity, ProtoModel.NewYearCardProto>();

            config.NewConfig<PlifeEntity, ProtoModel.PLifeProto>()
                .Map(dest => dest.MapId, src => src.Map)
                .Map(dest => dest.LifeId, src => src.Life);
            config.NewConfig<ProtoModel.PLifeProto, PlifeEntity>()
                .ConstructUsing(x => new PlifeEntity(x.Id, x.MapId, x.LifeId, x.Mobtime, x.X, x.Y, x.Fh, x.Type))
                .Map(dest => dest.Map, src => src.MapId)
                .Map(dest => dest.Life, src => src.LifeId);
        }
    }
}
