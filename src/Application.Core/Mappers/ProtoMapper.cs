using Application.Core.Game.Items;
using Application.Core.Game.Life;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Core.Game.Trades;
using Application.Core.Model;
using Application.Core.Models;
using client.inventory;
using Google.Protobuf.WellKnownTypes;
using net.server;
using server;
using server.life;
using server.maps;

namespace Application.Core.Mappers
{
    /// <summary>
    /// Proto &lt;-&gt; Object
    /// </summary>
    public class ProtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Timestamp, DateTimeOffset?>()
                .MapWith(src => src == null ? (DateTimeOffset?)null : src.ToDateTimeOffset());
            config.NewConfig<DateTimeOffset?, Timestamp>()
                .MapWith(src => src.HasValue ? Timestamp.FromDateTimeOffset(src.Value) : null!);

            config.NewConfig<Timestamp, DateTimeOffset>()
                .MapWith(src => src.ToDateTimeOffset());
            config.NewConfig<DateTimeOffset, Timestamp>()
                .MapWith(src => Timestamp.FromDateTimeOffset(src));

            config.NewConfig<DateTime, Timestamp>().MapWith(src => Timestamp.FromDateTime(src.ToUniversalTime()));
            config.NewConfig<Timestamp, DateTime>().MapWith(src => src.ToDateTime());

            config.NewConfig<RankProto.RankCharacter, RankedCharacterInfo>()
                .Map(dest => dest.Rank, x => x.Rank)
                .Map(dest => dest.CharacterName, x => x.Name)
                .Map(dest => dest.CharacterLevel, x => x.Level);

            config.NewConfig<Dto.CharacterDto, Player>()
                            .ConstructUsing(src => new Player())
                            .Map(x => x.MesoValue, x => new AtomicInteger(x.Meso))
                            .Map(x => x.ExpValue, x => new AtomicInteger(x.Exp))
                            .Map(x => x.GachaExpValue, x => new AtomicInteger(x.Gachaexp))
                            .Map(x => x.RemainingSp, x => TranslateArray(x.Sp))
                            .Map(x => x.HP, x => x.Hp)
                            .Map(x => x.MP, x => x.Mp)
                            .Map(x => x.MaxHP, x => x.Maxhp)
                            .Map(x => x.MaxMP, x => x.Maxmp)
                            .AfterMapping((before, after) => after.Reload());
            config.NewConfig<Player, Dto.CharacterDto>()
                            .Map(x => x.Sp, x => string.Join(",", x.RemainingSp))
                            .Map(x => x.Meso, x => x.MesoValue.get())
                            .Map(x => x.Exp, x => x.ExpValue.get())
                            .Map(x => x.Gachaexp, x => x.GachaExpValue.get())
                            .Map(x => x.BuddyCapacity, x => x.BuddyList.Capacity)
                            .Map(x => x.Equipslots, x => x.Bag[InventoryType.EQUIP].getSlotLimit())
                            .Map(x => x.Useslots, x => x.Bag[InventoryType.USE].getSlotLimit())
                            .Map(x => x.Etcslots, x => x.Bag[InventoryType.ETC].getSlotLimit())
                            .Map(x => x.Setupslots, x => x.Bag[InventoryType.SETUP].getSlotLimit())
                            .Map(x => x.MountLevel, x => x.MountModel == null ? 1 : x.MountModel.getLevel())
                            .Map(x => x.MountExp, x => x.MountModel == null ? 0 : x.MountModel.getExp())
                            .Map(x => x.Mounttiredness, x => x.MountModel == null ? 0 : x.MountModel.getTiredness())
                            .Map(x => x.Hp, x => x.HP)
                            .Map(x => x.Mp, x => x.MP)
                            .Map(x => x.Maxhp, x => x.MaxHP)
                            .Map(x => x.Maxmp, x => x.MaxMP);
            config.NewConfig<Dto.AccountCtrlDto, AccountCtrl>().TwoWays();

            #region Item
            config.NewConfig<Dto.ItemDto, Pet>()
                 .ConstructUsing(source => new Pet(source.Itemid, (short)source.Position, source.PetInfo!.Petid))
                .Map(x => x.Fullness, x => Math.Min(Limits.MaxFullness, x.PetInfo!.Fullness))
                .Map(x => x.Level, x => Math.Min(Limits.MaxPetLevel, x.PetInfo!.Level))
                .Map(x => x.Tameness, x => Math.Min(Limits.MaxTameness, x.PetInfo!.Closeness))
                .Map(x => x.PetAttribute, x => x.PetInfo!.Flag)
                .Map(x => x.Summoned, x => x.PetInfo!.Summoned)
                .AfterMapping((rs, dest) =>
                {
                    dest.setOwner(rs.Owner);
                    dest.setQuantity((short)rs.Quantity);
                    dest.setFlag((short)rs.Flag);
                    dest.setExpiration(rs.Expiration);
                    dest.setGiftFrom(rs.GiftFrom);

                    dest.setName(rs.PetInfo!.Name ?? "");
                }).Compile();
            config.NewConfig<Pet, Dto.ItemDto>()
                .Map(x => x.PetInfo, x => new Dto.PetDto
                {
                    Closeness = Math.Min(Limits.MaxTameness, x.Tameness),
                    Fullness = Math.Min(Limits.MaxFullness, x.Fullness),
                    Level = Math.Min(Limits.MaxPetLevel, (int)x.Level),
                    Flag = x.PetAttribute,
                    Name = x.getName(),
                    Summoned = x.Summoned,
                    Petid = x.getUniqueId()
                });

            config.NewConfig<Dto.ItemDto, Item>()
                .MapWith(src => MapItem(src))
                .AfterMapping((rs, dest, ctx) =>
                {
                    dest.setOwner(rs.Owner);
                    dest.setQuantity((short)rs.Quantity);
                    dest.setFlag((short)rs.Flag);
                    dest.setExpiration(rs.Expiration);
                    dest.setGiftFrom(rs.GiftFrom);
                });
            config.NewConfig<Item, Dto.ItemDto>()
                .Map(dest => dest.Owner, x => x.getOwner())
                .Map(dest => dest.Itemid, x => x.getItemId())
                .Map(dest => dest.Quantity, x => x.getQuantity())
                .Map(dest => dest.Flag, x => x.getFlag())
                .Map(dest => dest.Expiration, x => x.getExpiration())
                .Map(dest => dest.GiftFrom, x => x.getGiftFrom())
                .Map(dest => dest.Position, x => x.getPosition())
                .Map(dest => dest.InventoryType,
                    src => MapContext.Current == null
                        ? (sbyte)src.getInventoryType()
                        : Convert.ToSByte(MapContext.Current.Parameters.GetValueOrDefault("InventoryType", (sbyte)src.getInventoryType())))
                .Map(dest => dest.Type,
                    src => MapContext.Current == null
                        ? (int)ItemType.Inventory
                        : Convert.ToSByte(MapContext.Current.Parameters.GetValueOrDefault("Type", (int)ItemType.Inventory)));

            config.NewConfig<ItemProto.RingDto, RingSourceModel>().TwoWays();

            config.NewConfig<Dto.ItemDto, Equip>()
                    .ConstructUsing(source => new Equip(source.Itemid, (short)source.Position))
                    .AfterMapping((rs, dest) =>
                    {
                        dest.setOwner(rs.Owner);
                        dest.setQuantity((short)rs.Quantity);
                        dest.setFlag((short)rs.Flag);
                        dest.setExpiration(rs.Expiration);
                        dest.setGiftFrom(rs.GiftFrom);

                        dest.setAcc(rs.EquipInfo!.Acc);
                        dest.setAvoid(rs.EquipInfo!.Avoid);
                        dest.setDex(rs.EquipInfo!.Dex);
                        dest.setHands(rs.EquipInfo!.Hands);
                        dest.setHp(rs.EquipInfo!.Hp);
                        dest.setInt(rs.EquipInfo!.Int);
                        dest.setJump(rs.EquipInfo!.Jump);
                        dest.setVicious(rs.EquipInfo!.Vicious);
                        dest.setLuk(rs.EquipInfo!.Luk);
                        dest.setMatk(rs.EquipInfo!.Matk);
                        dest.setMdef(rs.EquipInfo!.Mdef);
                        dest.setMp(rs.EquipInfo!.Mp);
                        dest.setSpeed(rs.EquipInfo!.Speed);
                        dest.setStr(rs.EquipInfo!.Str);
                        dest.setWatk(rs.EquipInfo!.Watk);
                        dest.setWdef(rs.EquipInfo!.Wdef);
                        dest.setUpgradeSlots(rs.EquipInfo!.Upgradeslots);
                        dest.setLevel((byte)rs.EquipInfo!.Level);
                        dest.setItemExp(rs.EquipInfo!.Itemexp);
                        dest.setItemLevel((byte)rs.EquipInfo!.Itemlevel);

                        dest.SetRing(rs.EquipInfo!.RingId, rs.EquipInfo!.RingSourceInfo.Adapt<RingSourceModel>());
                    });
            config.NewConfig<Equip, Dto.ItemDto>()
                .Map(dest => dest.EquipInfo, src => new Dto.EquipDto
                {
                    Acc = src.getAcc(),
                    Avoid = src.getAvoid(),
                    Dex = src.getDex(),
                    Hands = src.getHands(),
                    Int = src.getInt(),
                    Luk = src.getLuk(),
                    Str = src.getStr(),
                    Hp = src.getHp(),
                    Mp = src.getMp(),
                    Watk = src.getWatk(),
                    Matk = src.getMatk(),
                    Speed = src.getSpeed(),
                    Wdef = src.getWdef(),
                    Mdef = src.getMdef(),
                    Upgradeslots = src.getUpgradeSlots(),
                    Level = src.getLevel(),
                    Itemlevel = src.getItemLevel(),
                    Itemexp = src.getItemExp(),
                    Jump = src.getJump(),
                    Vicious = src.getVicious(),
                    RingId = src.RingId,
                    RingSourceInfo = src.RingSource.Adapt<ItemProto.RingDto>(),
                });
            #endregion 

            config.NewConfig<Dto.StorageDto, Storage>()
                .ConstructUsing(x => new Storage(x.Accountid, (byte)x.Slots, x.Meso));
            config.NewConfig<Storage, Dto.StorageDto>()
                .Map(dest => dest.Meso, x => x.getMeso())
                .Map(dest => dest.Slots, x => x.getSlots())
                .Map(dest => dest.Accountid, x => x.AccountId);

            config.NewConfig<Dto.SkillMacroDto, SkillMacro>().TwoWays();
            config.NewConfig<Dto.FameLogRecordDto, FameLogObject>().TwoWays();
            config.NewConfig<Dto.BuddyDto, BuddyCharacter>().TwoWays();

            config.NewConfig<PlayerCoolDownValueHolder, Dto.CoolDownDto>()
                .Map(dest => dest.SkillId, x => x.skillId)
                .Map(dest => dest.StartTime, x => x.startTime)
                .Map(dest => dest.Length, x => x.length);

            config.NewConfig<Dto.DropItemDto, DropEntry>()
                .MapWith(src => MapDrop(src));

            config.NewConfig<Dto.NoteDto, NoteObject>()
                .Map(x => x.From, x => x.FromId < 0 ? LifeFactory.Instance.GetNPCStats(x.FromId).getName() : x.From);
            config.NewConfig<Dto.ShopDto, Shop>()
                .ConstructUsing((src, ctx) => new Shop(src.ShopId, src.NpcId, src.Items.Adapt<List<ShopItem>>()));
            config.NewConfig<Dto.ShopItemDto, ShopItem>()
                .ConstructUsing((src, ctx) => new ShopItem((short)src.Buyable, src.ItemId, src.Price, src.Pitch));

            config.NewConfig<ItemProto.GiftDto, GiftModel>();
            config.NewConfig<CashProto.SpecialCashItemDto, SpecialCashItem>()
                .ConstructUsing((src, ctx) => new SpecialCashItem(src.Sn, src.Modifier, (byte)src.Info));

            config.NewConfig<TeamProto.TeamMemberDto, TeamMember>();

            config.NewConfig<GuildProto.GuildMemberDto, GuildMember>();
            config.NewConfig<AllianceProto.AllianceDto, Alliance>()
                .ConstructUsing(src => new Alliance(src.AllianceId))
                .Map(dest => dest.RankTitles, src => src.RankTitles.ToArray());
            // Guild Team构造函数特殊，不通过映射
            config.NewConfig<Dto.NewYearCardDto, NewYearCardObject>();

            config.NewConfig<PlayerShopItem, ItemProto.PlayerShopItemDto>()
                .Map(dest => dest.Bundles, x => x.getBundles())
                .Map(dest => dest.Price, x => x.getPrice())
                .Map(dest => dest.Item, x => x.getItem());
            config.NewConfig<ItemProto.PlayerShopItemDto, PlayerShopItem>()
                .ConstructUsing((src, ctx) => new PlayerShopItem(src.Item.Adapt<Item>(), (short)src.Bundles, src.Price));

            config.NewConfig<ItemProto.RemoteHiredMerchantDto, RemoteHiredMerchantData>()
                .Map(dest => dest.Mesos, x => x.Meso)
                .Map(dest => dest.MapName, x => MapFactory.Instance.loadPlaceName(x.MapId));
            config.NewConfig<ItemProto.OwlSearchResultItemDto, OwlSearchResultItem>();
            config.NewConfig<ItemProto.OwlSearchResponse, OwlSearchResult>();

            config.NewConfig<ItemProto.GachaponPoolDto, GachaponDataObject>();
            config.NewConfig<ItemProto.GachaponPoolChanceDto, GachaponPoolLevelChanceDataObject>();
            config.NewConfig<ItemProto.GachaponPoolItemDto, GachaponPoolItemDataObject>();
        }

        private int[] TranslateArray(string str)
        {
            return str.Split(",").Select(int.Parse).ToArray();
        }

        Item MapItem(Dto.ItemDto src)
        {
            var mit = src.InventoryType.GetByType();
            if (mit == InventoryType.EQUIP || mit == InventoryType.EQUIPPED)
                return src.Adapt<Equip>();

            if (src.PetInfo != null)
                return src.Adapt<Pet>();

            return new Item(src.Itemid, (short)src.Position, (short)src.Quantity);
        }

        DropEntry MapDrop(Dto.DropItemDto src)
        {
            if (src.Type == (int)DropType.ReactorDrop)
                return DropEntry.ReactorDrop(src.DropperId, src.ItemId, src.Chance, (short)src.QuestId);
            if (src.Type == (int)DropType.MonsterDrop)
                return DropEntry.MobDrop(src.DropperId, src.ItemId, src.Chance, src.MinCount, src.MaxCount, (short)src.QuestId);
            if (src.Type == (int)DropType.GlobalDrop)
                return DropEntry.Global(src.DropperId, src.ItemId, src.Chance, src.MinCount, src.MaxCount, (short)src.QuestId);
            throw new BusinessFatalException("不支持的掉落类型");
        }
    }
}
