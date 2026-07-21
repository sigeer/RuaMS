using Application.Core.Login.Mappers;
using Application.Core.Login.Models;
using Dto;
using ItemProto;

namespace Application.Core.Login.Mappers
{
    public partial class ItemMapper : IItemMapper
    {
        public ItemDto MapToDto(ItemModel p1)
        {
            return p1 == null ? null : new ItemDto()
            {
                Type = (int)p1.Type,
                Characterid = p1.Characterid == null ? null : (int?)(int)p1.Characterid,
                Accountid = p1.Accountid == null ? null : (int?)(int)p1.Accountid,
                Itemid = p1.Itemid,
                InventoryType = (int)p1.InventoryType,
                Position = (int)p1.Position,
                Quantity = (int)p1.Quantity,
                Owner = p1.Owner,
                Flag = (int)p1.Flag,
                Expiration = p1.Expiration,
                GiftFrom = p1.GiftFrom,
                EquipInfo = p1.EquipInfo == null ? null : new EquipDto()
                {
                    Id = p1.EquipInfo.Id,
                    Upgradeslots = p1.EquipInfo.Upgradeslots,
                    Level = (int)p1.EquipInfo.Level,
                    Str = p1.EquipInfo.Str,
                    Dex = p1.EquipInfo.Dex,
                    Int = p1.EquipInfo.Int,
                    Luk = p1.EquipInfo.Luk,
                    Hp = p1.EquipInfo.Hp,
                    Mp = p1.EquipInfo.Mp,
                    Watk = p1.EquipInfo.Watk,
                    Matk = p1.EquipInfo.Matk,
                    Wdef = p1.EquipInfo.Wdef,
                    Mdef = p1.EquipInfo.Mdef,
                    Acc = p1.EquipInfo.Acc,
                    Avoid = p1.EquipInfo.Avoid,
                    Hands = p1.EquipInfo.Hands,
                    Speed = p1.EquipInfo.Speed,
                    Jump = p1.EquipInfo.Jump,
                    Locked = p1.EquipInfo.Locked,
                    Vicious = p1.EquipInfo.Vicious,
                    Itemlevel = (int)p1.EquipInfo.Itemlevel,
                    Itemexp = p1.EquipInfo.Itemexp,
                    RingSourceInfo = p1.EquipInfo.RingSourceInfo == null ? null : new RingDto()
                    {
                        Id = p1.EquipInfo.RingSourceInfo.Id,
                        CharacterId1 = p1.EquipInfo.RingSourceInfo.CharacterId1,
                        CharacterId2 = p1.EquipInfo.RingSourceInfo.CharacterId2,
                        RingId1 = p1.EquipInfo.RingSourceInfo.RingId1,
                        RingId2 = p1.EquipInfo.RingSourceInfo.RingId2,
                        ItemId = p1.EquipInfo.RingSourceInfo.ItemId
                    },
                    RingId = p1.EquipInfo.RingId
                },
                PetInfo = p1.PetInfo == null ? null : new PetDto()
                {
                    Petid = p1.PetInfo.Petid,
                    Name = p1.PetInfo.Name,
                    Level = p1.PetInfo.Level,
                    Closeness = p1.PetInfo.Closeness,
                    Fullness = p1.PetInfo.Fullness,
                    Summoned = p1.PetInfo.Summoned,
                    Flag = p1.PetInfo.Flag
                },
                UniqueId = p1.UniqueId,
                Properties = p1.Properties
            };
        }
        public ItemModel MapToObject(ItemDto p2)
        {
            return p2 == null ? null : new ItemModel()
            {
                Type = (byte)p2.Type,
                Characterid = p2.Characterid == null ? null : (int?)(int)p2.Characterid,
                Accountid = p2.Accountid == null ? null : (int?)(int)p2.Accountid,
                Itemid = p2.Itemid,
                InventoryType = (sbyte)p2.InventoryType,
                Position = (short)p2.Position,
                Quantity = (short)p2.Quantity,
                Owner = p2.Owner,
                Flag = (short)p2.Flag,
                Expiration = p2.Expiration,
                GiftFrom = p2.GiftFrom,
                UniqueId = p2.UniqueId,
                EquipInfo = p2.EquipInfo == null ? null : new EquipModel()
                {
                    Id = p2.EquipInfo.Id,
                    Upgradeslots = p2.EquipInfo.Upgradeslots,
                    Level = (byte)p2.EquipInfo.Level,
                    Str = p2.EquipInfo.Str,
                    Dex = p2.EquipInfo.Dex,
                    Int = p2.EquipInfo.Int,
                    Luk = p2.EquipInfo.Luk,
                    Hp = p2.EquipInfo.Hp,
                    Mp = p2.EquipInfo.Mp,
                    Watk = p2.EquipInfo.Watk,
                    Matk = p2.EquipInfo.Matk,
                    Wdef = p2.EquipInfo.Wdef,
                    Mdef = p2.EquipInfo.Mdef,
                    Acc = p2.EquipInfo.Acc,
                    Avoid = p2.EquipInfo.Avoid,
                    Hands = p2.EquipInfo.Hands,
                    Speed = p2.EquipInfo.Speed,
                    Jump = p2.EquipInfo.Jump,
                    Locked = p2.EquipInfo.Locked,
                    Vicious = p2.EquipInfo.Vicious,
                    Itemlevel = (byte)p2.EquipInfo.Itemlevel,
                    Itemexp = p2.EquipInfo.Itemexp,
                    RingId = p2.EquipInfo.RingId,
                    RingSourceInfo = p2.EquipInfo.RingSourceInfo == null ? null : new RingSourceModel()
                    {
                        Id = p2.EquipInfo.RingSourceInfo.Id,
                        ItemId = p2.EquipInfo.RingSourceInfo.ItemId,
                        RingId1 = p2.EquipInfo.RingSourceInfo.RingId1,
                        RingId2 = p2.EquipInfo.RingSourceInfo.RingId2,
                        CharacterId1 = p2.EquipInfo.RingSourceInfo.CharacterId1,
                        CharacterId2 = p2.EquipInfo.RingSourceInfo.CharacterId2
                    }
                },
                PetInfo = p2.PetInfo == null ? null : new PetModel()
                {
                    Petid = p2.PetInfo.Petid,
                    Name = p2.PetInfo.Name,
                    Level = p2.PetInfo.Level,
                    Closeness = p2.PetInfo.Closeness,
                    Fullness = p2.PetInfo.Fullness,
                    Summoned = p2.PetInfo.Summoned,
                    Flag = p2.PetInfo.Flag
                },
                Properties = p2.Properties
            };
        }
    }
}