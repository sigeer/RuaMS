using System;
using Application.Core.Game.Items;
using Application.Core.Mappers;
using client.inventory;
using Dto;
using ItemProto;

namespace Application.Core.Mappers
{
    public partial class ItemMapper : IItemMapper
    {
        public ItemDto MapToDto(Item p1)
        {
            Pet p2 = p1 as Pet;
            
            if (p2 != null)
            {
                return funcMain1(p2);
            }
            Equip p3 = p1 as Equip;
            
            if (p3 != null)
            {
                return funcMain3(p3);
            }
            
            if (p1 == null)
            {
                return null;
            }
            ItemDto result = new ItemDto();
            
            result.Type = p1.PlayerInventory == null ? -1 : (int)p1.PlayerInventory.StoreType;
            result.Itemid = p1.getItemId();
            result.InventoryType = (int)ProtoMapper.GetInventoryType(p1);
            result.Position = (int)p1.getPosition();
            result.Quantity = (int)p1.getQuantity();
            result.Owner = p1.getOwner();
            result.Flag = (int)p1.getFlag();
            result.Expiration = p1.getExpiration();
            result.GiftFrom = p1.getGiftFrom();
            result.UniqueId = p1.UniqueId;
            result.Properties = p1.Properties;
            return result;
            
        }
        public Item MapToObject(ItemDto src)
        {
            return ProtoMapper.MapItem(src);
        }
        
        private ItemDto funcMain1(Pet p4)
        {
            if (p4 == null)
            {
                return null;
            }
            ItemDto result = new ItemDto();
            
            result.Type = ((Item)p4).PlayerInventory == null ? -1 : (int)((Item)p4).PlayerInventory.StoreType;
            result.Itemid = ((Item)p4).getItemId();
            result.InventoryType = (int)ProtoMapper.GetInventoryType((Item)p4);
            result.Position = (int)((Item)p4).getPosition();
            result.Quantity = (int)((Item)p4).getQuantity();
            result.Owner = ((Item)p4).getOwner();
            result.Flag = (int)((Item)p4).getFlag();
            result.Expiration = ((Item)p4).getExpiration();
            result.GiftFrom = ((Item)p4).getGiftFrom();
            result.PetInfo = funcMain2(new PetDto()
            {
                Closeness = Math.Min(30000, p4.Tameness),
                Fullness = Math.Min(100, p4.Fullness),
                Level = Math.Min(30, (int)p4.Level),
                Flag = p4.PetAttribute,
                Name = p4.Name,
                Summoned = p4.Summoned,
                Petid = p4.getUniqueId()
            });
            result.UniqueId = p4.UniqueId;
            result.Properties = p4.Properties;
            return result;
            
        }
        
        private ItemDto funcMain3(Equip p6)
        {
            if (p6 == null)
            {
                return null;
            }
            ItemDto result = new ItemDto();
            
            result.Type = ((Item)p6).PlayerInventory == null ? -1 : (int)((Item)p6).PlayerInventory.StoreType;
            result.Itemid = ((Item)p6).getItemId();
            result.InventoryType = (int)ProtoMapper.GetInventoryType((Item)p6);
            result.Position = (int)((Item)p6).getPosition();
            result.Quantity = (int)((Item)p6).getQuantity();
            result.Owner = ((Item)p6).getOwner();
            result.Flag = (int)((Item)p6).getFlag();
            result.Expiration = ((Item)p6).getExpiration();
            result.GiftFrom = ((Item)p6).getGiftFrom();
            result.EquipInfo = p6 == null ? null : new EquipDto()
            {
                Upgradeslots = (int)p6.getUpgradeSlots(),
                Level = (int)p6.getLevel(),
                Str = p6.getStr(),
                Dex = p6.getDex(),
                Int = p6.getInt(),
                Luk = p6.getLuk(),
                Hp = p6.getHp(),
                Mp = p6.getMp(),
                Watk = p6.getWatk(),
                Matk = p6.getMatk(),
                Wdef = p6.getWdef(),
                Mdef = p6.getMdef(),
                Acc = p6.getAcc(),
                Avoid = p6.getAvoid(),
                Hands = p6.getHands(),
                Speed = p6.getSpeed(),
                Jump = p6.getJump(),
                Vicious = p6.getVicious(),
                Itemlevel = (int)p6.getItemLevel(),
                Itemexp = p6.getItemExp(),
                RingSourceInfo = p6.RingSource == null ? null : new RingDto()
                {
                    Id = p6.RingSource.Id,
                    CharacterId1 = p6.RingSource.CharacterId1,
                    CharacterId2 = p6.RingSource.CharacterId2,
                    RingId1 = p6.RingSource.RingId1,
                    RingId2 = p6.RingSource.RingId2,
                    ItemId = p6.RingSource.ItemId,
                    CharacterName1 = p6.RingSource.CharacterName1,
                    CharacterName2 = p6.RingSource.CharacterName2
                },
                RingId = p6.RingId
            };
            result.UniqueId = p6.UniqueId;
            result.Properties = p6.Properties;
            return result;
            
        }
        
        private PetDto funcMain2(PetDto p5)
        {
            return p5 == null ? null : new PetDto()
            {
                Petid = p5.Petid,
                Name = p5.Name,
                Level = p5.Level,
                Closeness = p5.Closeness,
                Fullness = p5.Fullness,
                Summoned = p5.Summoned,
                Flag = p5.Flag
            };
        }
    }
}