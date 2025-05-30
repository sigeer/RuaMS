using Application.Core.EF.Entities.Items;
using Application.Core.Login.Models;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Items;
using client.inventory;
using Microsoft.EntityFrameworkCore;

namespace Application.Core.Login.Datas
{
    public class InventoryManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="targetId"></param>
        /// <param name="isAccount"></param>
        /// <param name="itemType">需要满足IsAccount == isAccount</param>
        /// <returns></returns>
        public static List<ItemEntityPair> LoadItems(DBContext dbContext, int targetId, params ItemType[] itemFactories)
        {
            var itemType = itemFactories.Select(x => (int)x).ToArray();
            var items = (from a in dbContext.Inventoryitems.AsNoTracking()
                .Where(x => x.Characterid == targetId)
                .Where(x => itemType.Contains(x.Type))
                         join c in dbContext.Pets.AsNoTracking() on a.Petid equals c.Petid into css
                         from cs in css.DefaultIfEmpty()
                         select new { Item = a, Pet = cs }).ToList();

            var invItemId = items.Select(x => x.Item.Inventoryitemid).ToList();
            var equips = (from a in dbContext.Inventoryequipments.AsNoTracking().Where(x => invItemId.Contains(x.Inventoryitemid))
                          join e in dbContext.Rings.AsNoTracking() on a.RingId equals e.Id into ess
                          from es in ess.DefaultIfEmpty()
                          select new EquipEntityPair(a, es)).ToList();

            return (from a in items
                    join b in equips on a.Item.Inventoryitemid equals b.Equip.Inventoryitemid into bss
                    from bs in bss.DefaultIfEmpty()
                    select new ItemEntityPair(a.Item, bs, a.Pet)).ToList();
        }

        public static List<ItemEntityPair> LoadAccountItems(DBContext dbContext, int targetId, params ItemType[] itemFactories)
        {
            var itemType = itemFactories.Select(x => (int)x).ToArray();
            var items = (from a in dbContext.Inventoryitems.AsNoTracking()
                .Where(x => x.Accountid == targetId)
                .Where(x => itemType.Contains(x.Type))
                         join c in dbContext.Pets.AsNoTracking() on a.Petid equals c.Petid into css
                         from cs in css.DefaultIfEmpty()
                         select new { Item = a, Pet = cs }).ToList();

            var invItemId = items.Select(x => x.Item.Inventoryitemid).ToList();
            var equips = (from a in dbContext.Inventoryequipments.AsNoTracking().Where(x => invItemId.Contains(x.Inventoryitemid))
                          join e in dbContext.Rings.AsNoTracking() on a.RingId equals e.Id into ess
                          from es in ess.DefaultIfEmpty()
                          select new EquipEntityPair(a, es)).ToList();

            return (from a in items
                    join b in equips on a.Item.Inventoryitemid equals b.Equip.Inventoryitemid into bss
                    from bs in bss.DefaultIfEmpty()
                    select new ItemEntityPair(a.Item, bs, a.Pet)).ToList();
        }

        public static void CommitInventoryByType(DBContext dbContext, int targetId, ItemModel[] items, ItemFactory type)
        {
            var allItems = dbContext.Inventoryitems.Where(x => (type.IsAccount ? x.Accountid == targetId : x.Characterid == targetId) && x.Type == type.getValue()).ToList();
            if (allItems.Count != 0)
            {
                var itemIds = allItems.Select(x => x.Inventoryitemid).ToArray();
                var ringIds = dbContext.Inventoryequipments.Where(x => itemIds.Contains(x.Inventoryitemid)).Select(x => x.RingId).ToArray();

                var petIds = allItems.Select(x => x.Petid).ToArray();
                dbContext.Inventoryitems.Where(x => itemIds.Contains(x.Inventoryitemid)).ExecuteDelete();
                dbContext.Inventoryequipments.Where(x => itemIds.Contains(x.Inventoryitemid)).ExecuteDelete();
                dbContext.Pets.Where(x => petIds.Contains(x.Petid)).ExecuteDelete();
                dbContext.Rings.Where(x => ringIds.Contains(x.Id)).ExecuteDelete();
            }

            foreach (var item in items)
            {
                var model = new Inventoryitem()
                {
                    Itemid = item.Itemid,
                    Accountid = type.IsAccount ? targetId : null,
                    Characterid = type.IsAccount ? null : targetId,
                    Expiration = item.Expiration,
                    Flag = item.Flag,
                    GiftFrom = item.GiftFrom,
                    Inventorytype = item.InventoryType,
                    Owner = item.Owner,
                    Petid = item.PetInfo == null ? -1 : item.PetInfo.Petid,
                    Position = item.Position,
                    Quantity = item.Quantity,
                    Type = item.Type,
                };
                dbContext.Inventoryitems.AddAsync(model);
                dbContext.SaveChanges();

                if (item.EquipInfo != null)
                {
                    dbContext.Inventoryequipments.Add(new Inventoryequipment(model.Inventoryitemid,
                        item.EquipInfo.Upgradeslots,
                        item.EquipInfo.Level,
                        item.EquipInfo.Str,
                        item.EquipInfo.Dex,
                        item.EquipInfo.Int,
                        item.EquipInfo.Luk,
                        item.EquipInfo.Hp,
                        item.EquipInfo.Mp,
                        item.EquipInfo.Watk,
                        item.EquipInfo.Matk,
                        item.EquipInfo.Wdef,
                        item.EquipInfo.Mdef,
                        item.EquipInfo.Acc,
                        item.EquipInfo.Avoid,
                        item.EquipInfo.Hands,
                        item.EquipInfo.Speed,
                        item.EquipInfo.Jump,
                        item.EquipInfo.Locked,
                        item.EquipInfo.Vicious,
                        item.EquipInfo.Itemlevel,
                        item.EquipInfo.Itemexp,
                        item.EquipInfo.RingInfo?.Id ?? -1));

                    if (item.EquipInfo.RingInfo != null)
                    {
                        dbContext.Rings.Add(new Ring_Entity(
                            item.EquipInfo.RingInfo.Id,
                            item.EquipInfo.RingInfo.ItemId,
                            item.EquipInfo.RingInfo.PartnerRingId,
                            item.EquipInfo.RingInfo.PartnerChrId,
                            item.EquipInfo.RingInfo.PartnerName));
                    }
                }
                if (item.PetInfo != null)
                {
                    dbContext.Pets.Add(
                        new PetEntity(item.PetInfo.Petid, item.PetInfo.Name, item.PetInfo.Level, item.PetInfo.Closeness, item.PetInfo.Fullness, item.PetInfo.Summoned, item.PetInfo.Flag));
                }
            }

            dbContext.SaveChanges();
        }

        public static async Task CommitInventoryByTypeAsync(DBContext dbContext, int targetId, ItemModel[] items, ItemFactory type)
        {
            var allItems = dbContext.Inventoryitems.Where(x => (type.IsAccount ? x.Accountid == targetId : x.Characterid == targetId) && x.Type == type.getValue()).ToList();
            if (allItems.Count != 0)
            {
                var itemIds = allItems.Select(x => x.Inventoryitemid).ToArray();
                var ringIds = dbContext.Inventoryequipments.Where(x => itemIds.Contains(x.Inventoryitemid)).Select(x => x.RingId).ToArray();

                var petIds = allItems.Select(x => x.Petid).ToArray();
                await dbContext.Inventoryitems.Where(x => itemIds.Contains(x.Inventoryitemid)).ExecuteDeleteAsync();
                await dbContext.Inventoryequipments.Where(x => itemIds.Contains(x.Inventoryitemid)).ExecuteDeleteAsync();
                await dbContext.Pets.Where(x => petIds.Contains(x.Petid)).ExecuteDeleteAsync();
                await dbContext.Rings.Where(x => ringIds.Contains(x.Id)).ExecuteDeleteAsync();
            }

            foreach (var item in items)
            {
                var model = new Inventoryitem()
                {
                    Itemid = item.Itemid,
                    Accountid = type.IsAccount ? targetId : null,
                    Characterid = type.IsAccount ? null : targetId,
                    Expiration = item.Expiration,
                    Flag = item.Flag,
                    GiftFrom = item.GiftFrom,
                    Inventorytype = item.InventoryType,
                    Owner = item.Owner,
                    Petid = item.PetInfo == null ? -1 : item.PetInfo.Petid,
                    Position = item.Position,
                    Quantity = item.Quantity,
                    Type = item.Type,
                };
                await dbContext.Inventoryitems.AddAsync(model);
                await dbContext.SaveChangesAsync();

                if (item.EquipInfo != null)
                {
                    dbContext.Inventoryequipments.Add(new Inventoryequipment(model.Inventoryitemid,
                        item.EquipInfo.Upgradeslots,
                        item.EquipInfo.Level,
                        item.EquipInfo.Str,
                        item.EquipInfo.Dex,
                        item.EquipInfo.Int,
                        item.EquipInfo.Luk,
                        item.EquipInfo.Hp,
                        item.EquipInfo.Mp,
                        item.EquipInfo.Watk,
                        item.EquipInfo.Matk,
                        item.EquipInfo.Wdef,
                        item.EquipInfo.Mdef,
                        item.EquipInfo.Acc,
                        item.EquipInfo.Avoid,
                        item.EquipInfo.Hands,
                        item.EquipInfo.Speed,
                        item.EquipInfo.Jump,
                        item.EquipInfo.Locked,
                        item.EquipInfo.Vicious,
                        item.EquipInfo.Itemlevel,
                        item.EquipInfo.Itemexp,
                        item.EquipInfo.RingInfo?.Id ?? -1));

                    if (item.EquipInfo.RingInfo != null)
                    {
                        dbContext.Rings.Add(new Ring_Entity(
                            item.EquipInfo.RingInfo.Id,
                            item.EquipInfo.RingInfo.ItemId,
                            item.EquipInfo.RingInfo.PartnerRingId,
                            item.EquipInfo.RingInfo.PartnerChrId,
                            item.EquipInfo.RingInfo.PartnerName));
                    }
                }
                if (item.PetInfo != null)
                {
                    await dbContext.Pets.AddAsync(
                        new PetEntity(item.PetInfo.Petid, item.PetInfo.Name, item.PetInfo.Level, item.PetInfo.Closeness, item.PetInfo.Fullness, item.PetInfo.Summoned, item.PetInfo.Flag));
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
