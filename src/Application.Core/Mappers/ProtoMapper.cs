using Application.Core.Duey;
using Application.Core.Game.Items;
using Application.Core.Game.Life;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Core.Model;
using Application.Core.Models;
using Application.Shared.Items;
using AutoMapper;
using client.inventory;
using Google.Protobuf.WellKnownTypes;
using net.server;
using Rank;
using server;

namespace Application.Core.Mappers
{
    /// <summary>
    /// Proto &lt;-&gt; Object
    /// </summary>
    public class ProtoMapper : Profile
    {
        public ProtoMapper()
        {
            CreateMap<Timestamp, DateTimeOffset?>()
                .ConvertUsing(src => src == null ? (DateTimeOffset?)null : src.ToDateTimeOffset());
            CreateMap<DateTimeOffset?, Timestamp>()
                .ConvertUsing(src => src.HasValue ? Timestamp.FromDateTimeOffset(src.Value) : null!);

            CreateMap<Timestamp, DateTimeOffset>()
                .ConvertUsing(src => src.ToDateTimeOffset());
            CreateMap<DateTimeOffset, Timestamp>()
                .ConvertUsing(src => Timestamp.FromDateTimeOffset(src));

            CreateMap<DateTime, Timestamp>().ConvertUsing(src => Timestamp.FromDateTime(src.ToUniversalTime()));
            CreateMap<Timestamp, DateTime>().ConvertUsing(src => src.ToDateTime());

            CreateMap<Rank.RankCharacter, RankedCharacterInfo>()
                .ForMember(dest => dest.Rank, src => src.MapFrom(x => x.Rank))
                .ForMember(dest => dest.CharacterName, src => src.MapFrom(x => x.Name))
                .ForMember(dest => dest.CharacterLevel, src => src.MapFrom(x => x.Level));

            CreateMap<Dto.CharacterDto, Player>()
                            .ForMember(x => x.MesoValue, opt => opt.MapFrom(x => new AtomicInteger(x.Meso)))
                            .ForMember(x => x.ExpValue, opt => opt.MapFrom(x => new AtomicInteger(x.Exp)))
                            .ForMember(x => x.GachaExpValue, opt => opt.MapFrom(x => new AtomicInteger(x.Gachaexp)))
                            .ForMember(x => x.RemainingSp, opt => opt.MapFrom(x => TranslateArray(x.Sp)))
                            .ForMember(x => x.HP, opt => opt.MapFrom(x => x.Hp))
                            .ForMember(x => x.MP, opt => opt.MapFrom(x => x.Mp))
                            .ForMember(x => x.MaxHP, opt => opt.MapFrom(x => x.Maxhp))
                            .ForMember(x => x.MaxMP, opt => opt.MapFrom(x => x.Maxmp))
                            .AfterMap((before, after) => after.Reload())
                            .ReverseMap()
                            .ForMember(x => x.Sp, opt => opt.MapFrom(x => string.Join(",", x.RemainingSp)))
                            .ForMember(x => x.Meso, opt => opt.MapFrom(x => x.MesoValue.get()))
                            .ForMember(x => x.Exp, opt => opt.MapFrom(x => x.ExpValue.get()))
                            .ForMember(x => x.Gachaexp, opt => opt.MapFrom(x => x.GachaExpValue.get()))
                            .ForMember(x => x.BuddyCapacity, opt => opt.MapFrom(x => x.BuddyList.Capacity))
                            .ForMember(x => x.Equipslots, opt => opt.MapFrom(x => x.Bag[InventoryType.EQUIP].getSlotLimit()))
                            .ForMember(x => x.Useslots, opt => opt.MapFrom(x => x.Bag[InventoryType.USE].getSlotLimit()))
                            .ForMember(x => x.Etcslots, opt => opt.MapFrom(x => x.Bag[InventoryType.ETC].getSlotLimit()))
                            .ForMember(x => x.Setupslots, opt => opt.MapFrom(x => x.Bag[InventoryType.SETUP].getSlotLimit()))
                            .ForMember(x => x.MountLevel, opt => opt.MapFrom(x => x.MountModel == null ? 1 : x.MountModel.getLevel()))
                            .ForMember(x => x.MountExp, opt => opt.MapFrom(x => x.MountModel == null ? 0 : x.MountModel.getExp()))
                            .ForMember(x => x.Mounttiredness, opt => opt.MapFrom(x => x.MountModel == null ? 0 : x.MountModel.getTiredness()))
                            .ForMember(x => x.MessengerId, opt => opt.MapFrom(x => x.Messenger == null ? 0 : x.Messenger.getId()))
                            .ForMember(x => x.MessengerPosition, opt => opt.MapFrom(x => x.Messenger == null ? 4 : x.MessengerPosition))
                            .ForMember(x => x.Hp, opt => opt.MapFrom(x => x.HP))
                            .ForMember(x => x.Mp, opt => opt.MapFrom(x => x.MP))
                            .ForMember(x => x.Maxhp, opt => opt.MapFrom(x => x.MaxHP))
                            .ForMember(x => x.Maxmp, opt => opt.MapFrom(x => x.MaxMP));

            #region Item
            CreateMap<Dto.ItemDto, Pet>()
                 .ConstructUsing(source => new Pet(source.Itemid, (short)source.Position, source.PetInfo!.Petid
                 ))
                .ForMember(x => x.Fullness, opt => opt.MapFrom(x => Math.Min(Limits.MaxFullness, x.PetInfo!.Fullness)))
                .ForMember(x => x.Level, opt => opt.MapFrom(x => Math.Min(Limits.MaxLevel, x.PetInfo!.Level)))
                .ForMember(x => x.Tameness, opt => opt.MapFrom(x => Math.Min(Limits.MaxTameness, x.PetInfo!.Closeness)))
                .ForMember(x => x.PetAttribute, opt => opt.MapFrom(x => x.PetInfo!.Flag))
                .ForMember(x => x.Summoned, opt => opt.MapFrom(x => x.PetInfo!.Summoned))
                .AfterMap((rs, dest) =>
                {
                    dest.setOwner(rs.Owner);
                    dest.setQuantity((short)rs.Quantity);
                    dest.setFlag((short)rs.Flag);
                    dest.setExpiration(rs.Expiration);
                    dest.setGiftFrom(rs.GiftFrom);

                    dest.setName(rs.PetInfo!.Name ?? "");
                })
                .ReverseMap()
                .ForMember(x => x.PetInfo, opt => opt.MapFrom(x => new Dto.PetDto
                {
                    Closeness = Math.Min(Limits.MaxTameness, x.Tameness),
                    Fullness = Math.Min(Limits.MaxFullness, x.Fullness),
                    Level = Math.Min(Limits.MaxLevel, (int)x.Level),
                    Flag = x.PetAttribute,
                    Name = x.getName(),
                    Summoned = x.Summoned,
                    Petid = x.getUniqueId()
                }));

            CreateMap<Dto.ItemDto, Item>()
                .ConstructUsing((src, ctx) =>
                {
                    var mit = src.InventoryType.GetByType();
                    if (mit == InventoryType.EQUIP || mit == InventoryType.EQUIPPED)
                        return ctx.Mapper.Map<Equip>(src);

                    if (src.PetInfo != null)
                        return ctx.Mapper.Map<Pet>(src);

                    return new Item(src.Itemid, (short)src.Position, (short)src.Quantity);
                })
                .AfterMap((rs, dest, ctx) =>
                {
                    dest.setOwner(rs.Owner);
                    dest.setQuantity((short)rs.Quantity);
                    dest.setFlag((short)rs.Flag);
                    dest.setExpiration(rs.Expiration);
                    dest.setGiftFrom(rs.GiftFrom);
                })
                .ReverseMap()
                .ForMember(dest => dest.Owner, source => source.MapFrom(x => x.getOwner()))
                .ForMember(dest => dest.Itemid, source => source.MapFrom(x => x.getItemId()))
                .ForMember(dest => dest.Quantity, source => source.MapFrom(x => x.getQuantity()))
                .ForMember(dest => dest.Flag, source => source.MapFrom(x => x.getFlag()))
                .ForMember(dest => dest.Expiration, source => source.MapFrom(x => x.getExpiration()))
                .ForMember(dest => dest.GiftFrom, source => source.MapFrom(x => x.getGiftFrom()))
                .ForMember(dest => dest.Position, source => source.MapFrom(x => x.getPosition()))
                .ForMember(dest => dest.InventoryType, source => source.MapFrom((src, dest, destMember, context) =>
                {
                    if (context.TryGetItems(out var items) && items.TryGetValue("InventoryType", out var invType))
                        return Convert.ToSByte(invType);
                    return 0;
                }))
                .ForMember(dest => dest.Type, source => source.MapFrom((src, dest, destMember, context) =>
                {
                    if (context.TryGetItems(out var items) && items.TryGetValue("Type", out var type))
                        return Convert.ToSByte(type);
                    return ItemFactory.INVENTORY.getValue();
                }))
                .Include<Equip, Dto.ItemDto>()
                .Include<Pet, Dto.ItemDto>();

            CreateMap<Dto.RingDto, Ring>()
                .ConstructUsing(x => new Ring(x.Id, x.PartnerRingId, x.PartnerChrId, x.ItemId, x.PartnerName))
                .ReverseMap()
                .ForMember(dest => dest.ItemId, source => source.MapFrom(x => x.getItemId()))
                .ForMember(dest => dest.PartnerRingId, source => source.MapFrom(x => x.getPartnerRingId()))
                .ForMember(dest => dest.PartnerChrId, source => source.MapFrom(x => x.getPartnerChrId()))
                .ForMember(dest => dest.PartnerName, source => source.MapFrom(x => x.getPartnerName()))
                .ForMember(dest => dest.Id, source => source.MapFrom(x => x.getRingId()));

            CreateMap<Dto.ItemDto, Equip>()
                    .ConstructUsing(source => new Equip(source.Itemid, (short)source.Position))
                    .AfterMap((rs, dest, ctx) =>
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

                        if (rs.EquipInfo!.RingInfo != null)
                            dest.Ring = ctx.Mapper.Map<Ring>(rs.EquipInfo!.RingInfo);
                    })
                    .ReverseMap()
                    .ForMember(dest => dest.EquipInfo, source => source.MapFrom(x => x));

            CreateMap<Equip, Dto.EquipDto>()
                .ForMember(dest => dest.Acc, source => source.MapFrom(x => x.getAcc()))
                .ForMember(dest => dest.Avoid, source => source.MapFrom(x => x.getAvoid()))
                .ForMember(dest => dest.Dex, source => source.MapFrom(x => x.getDex()))
                .ForMember(dest => dest.Hands, source => source.MapFrom(x => x.getHands()))
                .ForMember(dest => dest.Hp, source => source.MapFrom(x => x.getHp()))
                .ForMember(dest => dest.Int, source => source.MapFrom(x => x.getInt()))
                .ForMember(dest => dest.Jump, source => source.MapFrom(x => x.getJump()))
                .ForMember(dest => dest.Vicious, source => source.MapFrom(x => x.getVicious()))
                .ForMember(dest => dest.Luk, source => source.MapFrom(x => x.getLuk()))
                .ForMember(dest => dest.Matk, source => source.MapFrom(x => x.getMatk()))
                .ForMember(dest => dest.Mdef, source => source.MapFrom(x => x.getMdef()))
                .ForMember(dest => dest.Mp, source => source.MapFrom(x => x.getMp()))
                .ForMember(dest => dest.Speed, source => source.MapFrom(x => x.getSpeed()))
                .ForMember(dest => dest.Str, source => source.MapFrom(x => x.getStr()))
                .ForMember(dest => dest.Watk, source => source.MapFrom(x => x.getWatk()))
                .ForMember(dest => dest.Wdef, source => source.MapFrom(x => x.getWdef()))
                .ForMember(dest => dest.Upgradeslots, source => source.MapFrom(x => x.getUpgradeSlots()))
                .ForMember(dest => dest.Level, source => source.MapFrom(x => x.getLevel()))
                .ForMember(dest => dest.Itemlevel, source => source.MapFrom(x => x.getItemLevel()))
                .ForMember(dest => dest.Itemexp, source => source.MapFrom(x => x.getItemExp()))
                .ForMember(dest => dest.RingInfo, source => source.MapFrom(x => x.Ring));
            #endregion 

            CreateMap<Dto.StorageDto, Storage>()
                .ConstructUsing((x, ctx) =>
                {
                    return new Storage(x.Accountid, (byte)x.Slots, x.Meso);
                })
                .ReverseMap()
                .ForMember(dest => dest.Meso, source => source.MapFrom(x => x.getMeso()))
                .ForMember(dest => dest.Slots, source => source.MapFrom(x => x.getSlots()))
                .ForMember(dest => dest.Accountid, source => source.MapFrom(x => x.AccountId));

            CreateMap<Dto.SkillMacroDto, SkillMacro>().ReverseMap();

            CreateMap<PlayerCoolDownValueHolder, Dto.CoolDownDto>()
                .ForMember(dest => dest.SkillId, source => source.MapFrom(x => x.skillId))
                .ForMember(dest => dest.StartTime, source => source.MapFrom(x => x.startTime))
                .ForMember(dest => dest.Length, source => source.MapFrom(x => x.length));

            CreateMap<Dto.DropItemDto, DropEntry>()
                .ConstructUsing((src, ctx) =>
                {
                    if (src.Type == (int)DropType.ReactorDrop)
                        return DropEntry.ReactorDrop(src.DropperId, src.ItemId, src.Chance, (short)src.QuestId);
                    if (src.Type == (int)DropType.MonsterDrop)
                        return DropEntry.MobDrop(src.DropperId, src.ItemId, src.Chance, src.MinCount, src.MaxCount, (short)src.QuestId);
                    if (src.Type == (int)DropType.GlobalDrop)
                        return DropEntry.Global(0, src.ItemId, src.Chance, src.MinCount, src.MaxCount, (short)src.QuestId);
                    throw new BusinessFatalException("不支持的掉落类型");
                });
            CreateMap<Dto.DueyPackageDto, DueyPackageObject>();
            CreateMap<Dto.NoteDto, NoteObject>();
            CreateMap<Dto.ShopDto, Shop>()
                .ConstructUsing((src, ctx) => new Shop(src.ShopId, src.NpcId, ctx.Mapper.Map<List<ShopItem>>(src.Items)));
            CreateMap<Dto.ShopItemDto, ShopItem>()
                .ConstructUsing((src, ctx) => new ShopItem((short)src.Buyable, src.ItemId, src.Price, src.Pitch));

            CreateMap<Dto.GiftDto, GiftModel>();
            CreateMap<Dto.SpecialCashItemDto, SpecialCashItem>()
                .ConstructUsing((src, ctx) => new SpecialCashItem(src.Sn, src.Modifier, (byte)src.Info));

            CreateMap<Dto.TeamMemberDto, TeamMember>();
            CreateMap<Dto.TeamDto, Team>()
                .ConstructUsing(src => new Team(src.Id, src.LeaderId))
                .AfterMap((src, dest, ctx) =>
                {
                    foreach (var member in src.Members)
                    {
                        dest.addMember(ctx.Mapper.Map<TeamMember>(member));
                    }
                });

            CreateMap<Dto.GuildMemberDto, GuildMember>();
            CreateMap<Dto.GuildDto, Guild>()
                .ForMember(dest => dest.RankTitles, src => src.MapFrom(x => new string[5]
                {
                    x.Rank1Title,
                    x.Rank2Title,
                    x.Rank3Title,
                    x.Rank4Title,
                    x.Rank5Title
                }));
        }

        private int[] TranslateArray(string str)
        {
            return str.Split(",").Select(int.Parse).ToArray();
        }
    }
}
