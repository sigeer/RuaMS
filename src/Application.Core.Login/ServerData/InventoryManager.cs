using Application.Core.EF.Entities.Items;
using Application.Core.Login.Models;
using Application.EF;
using Application.EF.Entities;
using Application.Scripting.JS;
using Application.Shared.Items;
using AutoMapper;
using client.inventory;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Application.Core.Login.Datas
{
    public class InventoryManager
    {
        readonly MasterServer _server;
        readonly IMapper _mapper;

        public InventoryManager(MasterServer server, IMapper mapper)
        {
            _server = server;
            _mapper = mapper;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="isAccount"></param>
        /// <param name="targetId"></param>
        /// <param name="itemFactories"></param>
        /// <returns></returns>
        public List<ItemModel> LoadItems(DBContext dbContext, bool isAccount, int targetId, params ItemType[] itemFactories)
        {
            return LoadItems(dbContext, isAccount, [targetId], itemFactories);
        }

        public List<ItemModel> LoadItems(DBContext dbContext, bool isAccount, int[] targetId, params ItemType[] itemFactories)
        {
            var itemType = itemFactories.Select(x => (int)x).ToArray();
            var dataList = (from a in dbContext.Inventoryitems.AsNoTracking()
                .Where(x => isAccount ? targetId.Contains(x.Accountid!.Value) : targetId.Contains(x.Characterid!.Value))
                .Where(x => itemType.Contains(x.Type))
                            join b in dbContext.Inventoryequipments on a.Inventoryitemid equals b.Inventoryitemid into bss
                            from bs in bss.DefaultIfEmpty()
                            join c in dbContext.Pets.AsNoTracking() on a.Petid equals c.Petid into css
                            from cs in css.DefaultIfEmpty()
                            select new ItemEntityPair(a, bs, cs)).ToList();

            var ringIds = dataList.Where(x => x.Equip != null && x.Equip.RingId > 0).Select(x => x.Equip!.RingId).ToArray();
            var rings = _server.RingManager.LoadRings(ringIds);
            List<ItemModel> items = [];
            foreach (var item in dataList)
            {
                RingSourceModel? ring = null;
                if (item.Equip != null && item.Equip.RingId > 0)
                    ring = rings.FirstOrDefault(x => x.RingId1 == item.Equip.RingId || x.RingId2 == item.Equip.RingId);

                var obj = _mapper.Map<ItemModel>(item);
                if (obj.EquipInfo != null)
                {
                    obj.EquipInfo.RingSourceInfo = ring;
                }
                items.Add(obj);
            }
            return items;
        }

        public static void CommitInventoryByType(DBContext dbContext, int targetId, ItemModel[] items, ItemFactory type)
        {
            var itemType = (byte)type.getValue();

            var allItems = dbContext.Inventoryitems.Where(x => (type.IsAccount ? x.Accountid == targetId : x.Characterid == targetId) && x.Type == itemType).ToList();
            if (allItems.Count != 0)
            {
                var itemIds = allItems.Select(x => x.Inventoryitemid).ToArray();

                var petIds = allItems.Select(x => x.Petid).ToArray();
                dbContext.Inventoryitems.Where(x => itemIds.Contains(x.Inventoryitemid)).ExecuteDelete();
                dbContext.Inventoryequipments.Where(x => itemIds.Contains(x.Inventoryitemid)).ExecuteDelete();
                dbContext.Pets.Where(x => petIds.Contains(x.Petid)).ExecuteDelete();
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
                    Type = itemType,
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
                        item.EquipInfo.RingId));
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
            var itemType = (byte)type.getValue();

            var allItems = dbContext.Inventoryitems.Where(x => (type.IsAccount ? x.Accountid == targetId : x.Characterid == targetId) && x.Type == itemType).ToList();
            if (allItems.Count != 0)
            {
                var itemIds = allItems.Select(x => x.Inventoryitemid).ToArray();

                var petIds = allItems.Select(x => x.Petid).ToArray();
                await dbContext.Inventoryitems.Where(x => itemIds.Contains(x.Inventoryitemid)).ExecuteDeleteAsync();
                await dbContext.Inventoryequipments.Where(x => itemIds.Contains(x.Inventoryitemid)).ExecuteDeleteAsync();
                await dbContext.Pets.Where(x => petIds.Contains(x.Petid)).ExecuteDeleteAsync();
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
                    Type = itemType,
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
                        item.EquipInfo.RingId));
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
