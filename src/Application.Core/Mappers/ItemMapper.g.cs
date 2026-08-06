using System;
using Application.Core.Game.Items;
using Application.Core.Mappers;
using client.inventory;
using ProtoModel;

namespace Application.Core.Mappers
{
    public partial class ItemMapper : IItemMapper
    {
        public ItemProto MapToDto(Item p1)
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
            ItemProto result = new ItemProto();
            
            result.UniqueId = p1.UniqueId;
            result.Itemid = p1.getItemId();
            result.Quantity = (int)p1.getQuantity();
            result.Position = (int)p1.getPosition();
            result.Owner = p1.getOwner();
            result.Flag = (int)p1.getFlag();
            result.Expiration = p1.getExpiration();
            result.GiftFrom = p1.getGiftFrom();
            result.Properties = p1.Properties;
            return result;
            
        }
        public Item MapToObject(ItemProto src)
        {
            return ProtoMapper.MapItem(src);
        }
        
        private ItemProto funcMain1(Pet p4)
        {
            if (p4 == null)
            {
                return null;
            }
            ItemProto result = new ItemProto();
            
            result.UniqueId = p4.UniqueId;
            result.Itemid = ((Item)p4).getItemId();
            result.Quantity = (int)((Item)p4).getQuantity();
            result.Position = (int)((Item)p4).getPosition();
            result.Owner = ((Item)p4).getOwner();
            result.Flag = (int)((Item)p4).getFlag();
            result.Expiration = ((Item)p4).getExpiration();
            result.GiftFrom = ((Item)p4).getGiftFrom();
            result.PetInfo = funcMain2(new PetProto()
            {
                Closeness = Math.Min(30000, p4.Tameness),
                Fullness = Math.Min(100, p4.Fullness),
                Level = Math.Min(30, (int)p4.Level),
                Flag = p4.PetAttribute,
                Name = p4.Name,
                Summoned = p4.Summoned,
                Petid = p4.getUniqueId()
            });
            result.Properties = p4.Properties;
            return result;
            
        }
        
        private ItemProto funcMain3(Equip p6)
        {
            if (p6 == null)
            {
                return null;
            }
            ItemProto result = new ItemProto();
            
            result.UniqueId = p6.UniqueId;
            result.Itemid = ((Item)p6).getItemId();
            result.Quantity = (int)((Item)p6).getQuantity();
            result.Position = (int)((Item)p6).getPosition();
            result.Owner = ((Item)p6).getOwner();
            result.Flag = (int)((Item)p6).getFlag();
            result.Expiration = ((Item)p6).getExpiration();
            result.GiftFrom = ((Item)p6).getGiftFrom();
            result.EquipInfo = p6 == null ? null : new EquipProto()
            {
                Level = (int)p6.getLevel(),
                Upgradeslots = (int)p6.getUpgradeSlots(),
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
                Itemexp = p6.getItemExp()
            };
            result.Properties = p6.Properties;
            return result;
            
        }
        
        private PetProto funcMain2(PetProto p5)
        {
            return p5 == null ? null : new PetProto()
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