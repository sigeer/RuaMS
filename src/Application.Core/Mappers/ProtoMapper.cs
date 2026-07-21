using Application.Core.Channel.DataProviders;
using Application.Core.Channel.DueyService;
using Application.Core.Client.inventory;
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
                .ConstructUsing(x => new RankedCharacterInfo(x.Rank, x.Level, x.Name));

            config.NewConfig<Dto.CharacterDto, Player>()
                            .ConstructUsing(src => (Player)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Player)))
                            .Map(x => x.RemainingSp, x => TranslateArray(x.Sp))
                            .Map(dest => dest.JobModel, src => JobFactory.GetById(src.JobId))
                            .Ignore(dest => dest.JobId);

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
                            .Map(x => x.Maxmp, x => x.MaxMP)
                            .Ignore(x => x.Map);

            config.NewConfig<AccountDto.AccountInfoProto, AccountInfoModel>();

            #region Item
            config.NewConfig<Dto.ItemDto, Pet>()
                 .ConstructUsing(source => new Pet(ItemInformationProvider.getInstance().GetPetTemplate(source.Itemid)!, (short)source.Position, source.PetInfo!.Petid))
                .Map(x => x.Fullness, x => Math.Min(Limits.MaxFullness, x.PetInfo!.Fullness))
                .Map(x => x.Level, x => Math.Min(Limits.MaxPetLevel, x.PetInfo!.Level))
                .Map(x => x.Tameness, x => Math.Min(Limits.MaxTameness, x.PetInfo!.Closeness))
                .Map(x => x.PetAttribute, x => x.PetInfo!.Flag)
                .AfterMapping((rs, dest) =>
                {
                    dest.setOwner(rs.Owner);
                    dest.setQuantity((short)rs.Quantity);
                    dest.setFlag((short)rs.Flag);
                    dest.setExpiration(rs.Expiration);
                    dest.setGiftFrom(rs.GiftFrom);

                    dest.Name = rs.PetInfo!.Name;
                });

            config.NewConfig<Pet, Dto.ItemDto>()
                .Inherits<Item, Dto.ItemDto>()
                .Map(x => x.PetInfo, x => new Dto.PetDto
                {
                    Closeness = Math.Min(Limits.MaxTameness, x.Tameness),
                    Fullness = Math.Min(Limits.MaxFullness, x.Fullness),
                    Level = Math.Min(Limits.MaxPetLevel, (int)x.Level),
                    Flag = x.PetAttribute,
                    Name = x.Name,
                    Summoned = x.Summoned,
                    Petid = x.getUniqueId()
                });

            config.NewConfig<Dto.ItemDto, Item>()
                .MapWith(src => MapItem(src));

            config.NewConfig<Item, Dto.ItemDto>()
                .Map(dest => dest.Owner, source => source.getOwner())
                .Map(dest => dest.Itemid, source => source.getItemId())
                .Map(dest => dest.Quantity, source => source.getQuantity())
                .Map(dest => dest.Flag, source => source.getFlag())
                .Map(dest => dest.Expiration, source => source.getExpiration())
                .Map(dest => dest.GiftFrom, source => source.getGiftFrom())
                .Map(dest => dest.Position, source => source.getPosition())
                .Map(dest => dest.InventoryType, src => GetInventoryType(src))
                .Map(dest => dest.Type, src => src.PlayerInventory == null ? -1 : (int)src.PlayerInventory.StoreType)
                .Include<Pet, Dto.ItemDto>()
                .Include<Equip, Dto.ItemDto>();


            config.NewConfig<ItemProto.RingDto, RingSourceModel>().TwoWays();

            config.NewConfig<Dto.ItemDto, Equip>()
                    .ConstructUsing(source => new Equip(ItemInformationProvider.getInstance().GetEquipTemplate(source.Itemid)!, (short)source.Position, source.UniqueId))
                    .AfterMapping((rs, dest, ctx) =>
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
                .Inherits<Item, Dto.ItemDto>()
                .Map(dest => dest.EquipInfo, source => source);

            config.NewConfig<Equip, Dto.EquipDto>()
                .Map(dest => dest.Acc, source => source.getAcc())
                .Map(dest => dest.Avoid, source => source.getAvoid())
                .Map(dest => dest.Dex, source => source.getDex())
                .Map(dest => dest.Hands, source => source.getHands())
                .Map(dest => dest.Hp, source => source.getHp())
                .Map(dest => dest.Int, source => source.getInt())
                .Map(dest => dest.Jump, source => source.getJump())
                .Map(dest => dest.Vicious, source => source.getVicious())
                .Map(dest => dest.Luk, source => source.getLuk())
                .Map(dest => dest.Matk, source => source.getMatk())
                .Map(dest => dest.Mdef, source => source.getMdef())
                .Map(dest => dest.Mp, source => source.getMp())
                .Map(dest => dest.Speed, source => source.getSpeed())
                .Map(dest => dest.Str, source => source.getStr())
                .Map(dest => dest.Watk, source => source.getWatk())
                .Map(dest => dest.Wdef, source => source.getWdef())
                .Map(dest => dest.Upgradeslots, source => source.getUpgradeSlots())
                .Map(dest => dest.Level, source => source.getLevel())
                .Map(dest => dest.Itemlevel, source => source.getItemLevel())
                .Map(dest => dest.Itemexp, source => source.getItemExp())
                .Map(dest => dest.RingId, source => source.RingId)
                .Map(dest => dest.RingSourceInfo, source => source.RingSource);
            #endregion 

            config.NewConfig<Dto.SkillMacroDto, SkillMacro>()
                .ConstructUsing(x => new SkillMacro(x.Skill1, x.Skill1, x.Skill3, x.Name, x.Shout, x.Position));

            config.NewConfig<SkillMacro, Dto.SkillMacroDto>();

            config.NewConfig<Dto.FameLogRecordDto, FameLogObject>();
            config.NewConfig<FameLogObject, Dto.FameLogRecordDto>();

            config.NewConfig<BuddyProto.BuddyDto, BuddyCharacter>();
            config.NewConfig<BuddyCharacter, BuddyProto.BuddyDto>();

            config.NewConfig<PlayerCoolDownValueHolder, Dto.CoolDownDto>()
                .Map(dest => dest.SkillId, source => source.skillId)
                .Map(dest => dest.StartTime, source => source.startTime)
                .Map(dest => dest.Length, source => source.length);

            config.NewConfig<Dto.DropItemDto, DropEntry>()
                .MapWith(src => MapDrop(src));

            config.NewConfig<Dto.NoteDto, NoteObject>();

            config.NewConfig<Dto.ShopDto, Shop>()
                .ConstructUsing(src => new Shop(src.ShopId, src.NpcId, src.Items.Adapt<List<ShopItem>>()));
            config.NewConfig<Dto.ShopItemDto, ShopItem>()
                .ConstructUsing(src => new ShopItem((short)src.Buyable, src.ItemId, src.Price, src.Pitch));

            config.NewConfig<ItemProto.GiftDto, GiftModel>();
            config.NewConfig<CashProto.SpecialCashItemDto, SpecialCashItem>()
                .ConstructUsing(src => new SpecialCashItem(src.Sn, src.Modifier, (byte)src.Info));

            config.NewConfig<TeamProto.TeamMemberDto, TeamMember>()
                .Map(dest => dest.JobId, src => src.Job);

            config.NewConfig<GuildProto.GuildMemberDto, GuildMember>();

            config.NewConfig<Dto.NewYearCardDto, NewYearCardObject>();

            config.NewConfig<PlayerShopItem, ItemProto.PlayerShopItemDto>()
                .Map(dest => dest.Bundles, src => src.getBundles())
                .Map(dest => dest.Price, src => src.getPrice())
                .Map(dest => dest.Item, src => src.getItem());

            config.NewConfig<ItemProto.PlayerShopItemDto, PlayerShopItem>()
                .ConstructUsing(src => new PlayerShopItem(src.Item.Adapt<Item>(), (short)src.Bundles, src.Price));

            config.NewConfig<ItemProto.RemoteHiredMerchantDto, RemoteHiredMerchantData>();
            config.NewConfig<ItemProto.OwlSearchResultItemDto, OwlSearchResultItem>();
            config.NewConfig<ItemProto.OwlSearchResponse, OwlSearchResult>();

            config.NewConfig<ItemProto.GachaponPoolDto, GachaponDataObject>();
            config.NewConfig<ItemProto.GachaponPoolChanceDto, GachaponPoolLevelChanceDataObject>();
            config.NewConfig<ItemProto.GachaponPoolItemDto, GachaponPoolItemDataObject>();

            config.NewConfig<LifeProto.PlayerNPCEquip, PlayerNpcEquipObject>()
                .Map(dest => dest.EquipId, src => src.ItemId)
                .Map(dest => dest.EquipPos, src => src.Position);

            config.NewConfig<PlayerNpcEquipObject, LifeProto.PlayerNPCEquip>()
                .Map(dest => dest.ItemId, src => src.EquipId)
                .Map(dest => dest.Position, src => src.EquipPos);

            config.NewConfig<LifeProto.PlayerNPCDto, PlayerNpc>()
                .Map(dest => dest.NpcId, src => src.ScriptId)
                .AfterMapping((src, dest) =>
                {
                    dest.setObjectId(dest.Id);
                    dest.setPosition(new Point(dest.X, dest.Cy));
                });

            config.NewConfig<PlayerNpc, LifeProto.PlayerNPCDto>()
                .Map(dest => dest.ScriptId, src => src.NpcId);

            config.NewConfig<DueyDto.DueyPackageDto, DueyPackageObject>();
        }

        public static int[] TranslateArray(string str)
        {
            return str.Split(",").Select(int.Parse).ToArray();
        }

        public static sbyte GetInventoryType(Item src)
        {
            return src.PlayerInventory is AbstractInventory inv
                        ? (sbyte)inv.getType()
                        : (sbyte)src.getInventoryType();
        }

        public static Item? MapItem(Dto.ItemDto src)
        {
            if (src == null)
                return null;

            if (src.EquipInfo != null)
            {
                return src.Adapt<Equip>();
            }

            //if (src.InventoryType == (int)InventoryType.EQUIP || src.InventoryType == (int)InventoryType.EQUIPPED)
            //{
            //    var equip = ItemInformationProvider.getInstance().getEquipById(src.Itemid, (short)src.Position);
            //    return equip;
            //}

            if (src.PetInfo != null)
                return src.Adapt<Pet>();

            var dest = new Item(src.Itemid, (short)src.Position, (short)src.Quantity, src.UniqueId);
            dest.setOwner(src.Owner);
            dest.setFlag((short)src.Flag);
            dest.setExpiration(src.Expiration);
            dest.setGiftFrom(src.GiftFrom);
            dest.Properties = src.Properties;
            return dest;
        }

        public static DropEntry MapDrop(Dto.DropItemDto src)
        {
            if (src.Type == (int)DropFromType.ReactorDrop)
                return DropEntry.ReactorDrop(src.DropperId, src.ItemId, src.Chance, (short)src.QuestId);
            if (src.Type == (int)DropFromType.MonsterDrop)
                return DropEntry.MobDrop(src.DropperId, src.ItemId, src.Chance, src.MinCount, src.MaxCount, (short)src.QuestId);
            if (src.Type == (int)DropFromType.GlobalDrop)
                return DropEntry.Global(src.DropperId, src.ItemId, src.Chance, src.MinCount, src.MaxCount, (short)src.QuestId);
            throw new BusinessFatalException("不支持的掉落类型");
        }
    }
}
