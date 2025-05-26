using Application.Core.Game.Items;
using Application.Core.Game.Life;
using Application.Core.Game.Players;
using Application.Core.Game.Relation;
using Application.Core.Game.Skills;
using Application.Shared.Characters;
using Application.Shared.Items;
using Application.Utility.Compatible.Atomics;
using Application.Utility.Exceptions;
using AutoMapper;
using client.inventory;
using net.server;
using server;

namespace Application.Core.Channel.Mappers
{
    /// <summary>
    /// Object = Dto
    /// </summary>
    public class ObjectMapper : Profile
    {
        public ObjectMapper()
        {
            CreateMap<CharacterDto, Player>()
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

            CreateMap<ItemDto, Pet>()
                 .ConstructUsing(source => new Pet(source.Itemid, source.Position, source.Petid))
                .ForMember(x => x.Fullness, opt => opt.MapFrom(x => Math.Min(Limits.MaxFullness, x.PetInfo!.Fullness)))
                .ForMember(x => x.Level, opt => opt.MapFrom(x => Math.Min(Limits.MaxLevel, x.PetInfo!.Level)))
                .ForMember(x => x.Tameness, opt => opt.MapFrom(x => Math.Min(Limits.MaxTameness, x.PetInfo!.Closeness)))
                .ForMember(x => x.PetAttribute, opt => opt.MapFrom(x => x.PetInfo!.Flag))
                .AfterMap((rs, dest) =>
                {
                    //dest.setOwner(rs.Owner);
                    //dest.setQuantity(rs.Quantity);
                    //dest.setFlag(rs.Flag);
                    //dest.setExpiration(rs.Expiration);
                    //dest.setGiftFrom(rs.GiftFrom);
                    dest.setName(rs.PetInfo!.Name ?? "");
                })
                .ReverseMap()
                .ForMember(x => x.PetInfo, opt => opt.MapFrom(x => new PetDto
                {
                    Closeness = Math.Min(Limits.MaxTameness, x.Tameness),
                    Fullness = Math.Min(Limits.MaxFullness, x.Fullness),
                    Level = Math.Min(Limits.MaxLevel, (int)x.Level),
                    Flag = x.PetAttribute,
                    Name = x.getName(),
                    Summoned = x.Summoned,
                    Petid = x.getUniqueId()
                }));

            CreateMap<ItemDto, Item>()
                .ConstructUsing(source => new Item(source.Itemid, source.Position, source.Quantity))
                .AfterMap((rs, dest, ctx) =>
                {
                    dest.setOwner(rs.Owner);
                    dest.setQuantity(rs.Quantity);
                    dest.setFlag(rs.Flag);
                    dest.setExpiration(rs.Expiration);
                    dest.setGiftFrom(rs.GiftFrom);
                    if (rs.PetInfo != null)
                        dest.SetPet(ctx.Mapper.Map<Pet>(rs));
                })
                .ReverseMap()
                .ForMember(dest => dest.Owner, source => source.MapFrom(x => x.getOwner()))
                .ForMember(dest => dest.Itemid, source => source.MapFrom(x => x.getItemId()))
                .ForMember(dest => dest.Quantity, source => source.MapFrom(x => x.getQuantity()))
                .ForMember(dest => dest.Flag, source => source.MapFrom(x => x.getFlag()))
                .ForMember(dest => dest.Expiration, source => source.MapFrom(x => x.getExpiration()))
                .ForMember(dest => dest.GiftFrom, source => source.MapFrom(x => x.getGiftFrom()))
                .ForMember(dest => dest.PetInfo, source => source.MapFrom(x => x.getPet()))
                .ForMember(dest => dest.Position, source => source.MapFrom(x => x.getPosition()))
                .ForMember(dest => dest.InventoryType, source => source.MapFrom((src, dest, destMember, context) =>
                {
                    return context.Items.TryGetValue("InventoryType", out var invType) ? Convert.ToSByte(invType) : (sbyte)src.getInventoryType();
                }))
                .ForMember(dest => dest.Type, source => source.MapFrom((src, dest, destMember, context) =>
                {
                    return context.Items.TryGetValue("Type", out var type) ? Convert.ToByte(type) : ItemFactory.INVENTORY.getValue();
                }))
                .Include<Equip, ItemDto>();

            CreateMap<RingDto, Ring>()
                .ConstructUsing(x => new Ring(x.Id, x.PartnerRingId, x.PartnerChrId, x.ItemId, x.PartnerName))
                .ReverseMap()
                .ForMember(dest => dest.ItemId, source => source.MapFrom(x => x.getItemId()))
                .ForMember(dest => dest.PartnerRingId, source => source.MapFrom(x => x.getPartnerRingId()))
                .ForMember(dest => dest.PartnerChrId, source => source.MapFrom(x => x.getPartnerChrId()))
                .ForMember(dest => dest.PartnerName, source => source.MapFrom(x => x.getPartnerName()))
                .ForMember(dest => dest.Id, source => source.MapFrom(x => x.getRingId()));

            CreateMap<ItemDto, Equip>()
                    .ConstructUsing(source => new Equip(source.Itemid, source.Position))
                    .AfterMap((rs, dest, ctx) =>
                    {
                        dest.setOwner(rs.Owner);
                        dest.setQuantity(rs.Quantity);
                        dest.setFlag(rs.Flag);
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
                        dest.setLevel(rs.EquipInfo!.Level);
                        dest.setItemExp(rs.EquipInfo!.Itemexp);
                        dest.setItemLevel(rs.EquipInfo!.Itemlevel);
                        dest.setRingId(rs!.EquipInfo!.RingId);

                        if (rs.EquipInfo!.RingInfo != null)
                            dest.Ring = ctx.Mapper.Map<Ring>(rs.EquipInfo!.RingInfo);
                    })
                    .ReverseMap()
                    .ForMember(dest => dest.EquipInfo, source => source.MapFrom(x => x));

            CreateMap<Equip, EquipDto>()
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
                .ForMember(dest => dest.RingId, source => source.MapFrom(x => x.getRingId()))
                .ForMember(dest => dest.RingInfo, source => source.MapFrom(x => x.Ring));

            CreateMap<StorageDto, Storage>()
                .ConstructUsing((x, ctx) =>
                {
                    return new Storage(x.Accountid, x.Slots, x.Meso, x.Items.Select(y => MapToItem(ctx.Mapper, y)).ToArray());
                })
                .ReverseMap()
                .ForMember(dest => dest.Meso, source => source.MapFrom(x => x.getMeso()))
                .ForMember(dest => dest.Slots, source => source.MapFrom(x => x.getSlots()))
                .ForMember(dest => dest.Accountid, source => source.MapFrom(x => x.AccountId));

            CreateMap<SkillMacroDto, SkillMacro>().ReverseMap();

            CreateMap<PlayerCoolDownValueHolder, CoolDownDto>()
                .ForMember(dest => dest.SkillId, source => source.MapFrom(x => x.skillId))
                .ForMember(dest => dest.StartTime, source => source.MapFrom(x => x.startTime))
                .ForMember(dest => dest.Length, source => source.MapFrom(x => x.length));

            CreateMap<DropDto, DropEntry>()
                .ConstructUsing((src, ctx) =>
                {
                    if (src.Type == DropType.ReactorDrop)
                        return DropEntry.ReactorDrop(src.DropperId, src.ItemId, src.Chance, src.QuestId);
                    if (src.Type == DropType.MonsterDrop)
                        return DropEntry.MobDrop(src.DropperId, src.ItemId, src.Chance, src.MinCount, src.MaxCount, src.QuestId);
                    if (src.Type == DropType.GlobalDrop)
                        return DropEntry.Global(0, src.ItemId, src.Chance, src.MinCount, src.MaxCount, src.QuestId);
                    throw new BusinessFatalException("不支持的掉落类型");
                });
        }

        private int[] TranslateArray(string str)
        {
            return str.Split(",").Select(int.Parse).ToArray();
        }

        private Item MapToItem(IRuntimeMapper mapper, ItemDto itemDto)
        {
            InventoryType mit = itemDto.InventoryType.getByType();
            if (mit.Equals(InventoryType.EQUIP) || mit.Equals(InventoryType.EQUIPPED))
                return mapper.Map<Equip>(itemDto);
            else
                return mapper.Map<Item>(itemDto);
        }
    }
}
