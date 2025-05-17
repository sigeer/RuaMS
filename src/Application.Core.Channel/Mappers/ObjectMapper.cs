using Application.Core.Game.Items;
using Application.Core.Game.Players;
using Application.EF.Entities;
using Application.Shared.Characters;
using Application.Shared.Constants;
using Application.Shared.Items;
using Application.Utility.Compatible.Atomics;
using AutoMapper;
using client.inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                .ForMember(x => x.Hp, opt => opt.MapFrom(x => x.HP))
                .ForMember(x => x.Mp, opt => opt.MapFrom(x => x.MP))
                .ForMember(x => x.Maxhp, opt => opt.MapFrom(x => x.MaxHP))
                .ForMember(x => x.Maxmp, opt => opt.MapFrom(x => x.MaxMP));

            CreateMap<PetDto, Pet>()
                .ForMember(x => x.Fullness, opt => opt.MapFrom(x => Math.Min(Limits.MaxFullness, x.Fullness)))
                .ForMember(x => x.Level, opt => opt.MapFrom(x => Math.Min(Limits.MaxLevel, x.Level)))
                .ForMember(x => x.Tameness, opt => opt.MapFrom(x => Math.Min(Limits.MaxTameness, x.Closeness)))
                .ReverseMap()
                .ForMember(x => x.Fullness, opt => opt.MapFrom(x => Math.Min(Limits.MaxFullness, x.Fullness)))
                .ForMember(x => x.Level, opt => opt.MapFrom(x => Math.Min(Limits.MaxLevel, (int)x.Level)))
                .ForMember(x => x.Closeness, opt => opt.MapFrom(x => Math.Min(Limits.MaxTameness, x.Tameness)));

            CreateMap<ItemDto, Item>()
                .ConstructUsing(source => new Item(source.Itemid,  source.Position, source.Quantity))
                .AfterMap((rs, dest, ctx) =>
                {
                    dest.setOwner(rs.Owner);
                    dest.setQuantity(rs.Quantity);
                    dest.setFlag(rs.Flag);
                    dest.setExpiration(rs.Expiration);
                    dest.setGiftFrom(rs.GiftFrom);
                    dest.SetPet(ctx.Mapper.Map<Pet>(rs.PetInfo));
                });

            CreateMap<ItemDto, Equip>()
                    .ConstructUsing(source => new Equip(source.Itemid, source.Position, 0))
                    .AfterMap((rs, dest) =>
                    {
                        dest.setOwner(rs.Owner);
                        dest.setQuantity(rs.Quantity);
                        dest.setAcc(rs.EquipInfo!.Acc);
                        dest.setAvoid(rs.EquipInfo!.Avoid);
                        dest.setDex(rs.EquipInfo!.Dex);
                        dest.setHands(rs.EquipInfo!.Hands);
                        dest.setHp(rs.EquipInfo!.Hp);
                        dest.setInt(rs.EquipInfo!.Int);
                        dest.setJump(rs.EquipInfo!.Jump);
                        dest.setVicious(rs.EquipInfo!.Vicious);
                        dest.setFlag(rs.Flag);
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
                        dest.setExpiration(rs.Expiration);
                        dest.setGiftFrom(rs.GiftFrom);
                        dest.setRingId(rs!.EquipInfo!.RingId);
                    });
        }

        private int[] TranslateArray(string str)
        {
            return str.Split(",").Select(int.Parse).ToArray();
        }
    }
}
