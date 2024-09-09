/*
 This file is part of the OdinMS Maple Story Server
 Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
 Matthias Butz <matze@odinms.de>
 Jan Christian Meyer <vimes@odinms.de>
 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU Affero General Public License as
 published by the Free Software Foundation version 3 as published by
 the Free Software Foundation. You may not use, modify or distribute
 this program under any other version of the GNU Affero General Public
 License.
 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU Affero General Public License for more details.
 You should have received a copy of the GNU Affero General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using client.inventory;
using constants.game;
using Microsoft.EntityFrameworkCore;
using provider;
using provider.wz;
using tools;

namespace server;



/**
 * @author Matze
 */
public class Storage
{
    private ILogger log;
    private static Dictionary<int, int> trunkGetCache = new();
    private static Dictionary<int, int> trunkPutCache = new();

    private int id;
    private int currentNpcid;
    private int meso;
    private byte slots;
    private Dictionary<InventoryType, List<Item>> typeItems = new();
    private List<Item> items = new();
    private object lockObj = new object();
    public bool IsChanged { get; set; }

    private Storage(int id, byte slots, int meso)
    {
        log = LogFactory.GetLogger("Storage");
        this.id = id;
        this.slots = slots;
        this.meso = meso;
    }

    private static Storage create(int id, int world)
    {
        using var dbContext = new DBContext();
        dbContext.Storages.Add(new DB_Storage()
        {
            Accountid = id,
            World = world,
            Slots = 4,
            Meso = 0
        });
        dbContext.SaveChanges();

        return loadOrCreateFromDB(id, world);
    }

    public static Storage loadOrCreateFromDB(int id, int world)
    {
        Storage ret;
        try
        {
            using var dbContext = new DBContext();
            var accountStorage = dbContext.Storages.Where(x => x.Accountid == id && x.World == world).Select(x => new { x.Storageid, x.Slots, x.Meso }).FirstOrDefault();
            if (accountStorage != null)
            {
                ret = new Storage(accountStorage.Storageid, (byte)accountStorage.Slots, accountStorage.Meso);
                foreach (var item in ItemFactory.STORAGE.loadItems(ret.id, false))
                {
                    ret.items.Add(item.Item);
                }
            }
            else
            {
                ret = create(id, world);
            }
            return ret;
        }
        catch (Exception ex)
        {
            // exceptions leading to deploy null storages found thanks to Jefe
            Log.Logger.Error(ex, "SQL error occurred when trying to load storage for accId {AccountId}, world {WorldId}", id, GameConstants.WORLD_NAMES[world]);
            throw;
        }
    }

    public byte getSlots()
    {
        return slots;
    }

    public bool canGainSlots(int slots)
    {
        slots += this.slots;
        return slots <= 48;
    }

    public bool gainSlots(int slots)
    {
        Monitor.Enter(lockObj);
        try
        {
            if (canGainSlots(slots))
            {
                slots += this.slots;
                this.slots = (byte)slots;

                IsChanged = true;
                return true;
            }

            return false;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void saveToDB(DBContext dbContext)
    {
        dbContext.Storages.Where(x => x.Storageid == id).
            ExecuteUpdate(x => x.SetProperty(y => y.Slots, slots)
                .SetProperty(y => y.Meso, meso));

        List<ItemInventoryType> itemsWithType = new();

        var list = getItems();
        foreach (Item item in list)
        {
            itemsWithType.Add(new(item, item.getInventoryType()));
        }

        ItemFactory.STORAGE.saveItems(itemsWithType, id, dbContext);
        IsChanged = false;
    }

    public Item getItem(sbyte slot)
    {
        Monitor.Enter(lockObj);
        try
        {
            return items.get(slot);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public bool takeOut(Item item)
    {
        Monitor.Enter(lockObj);
        try
        {
            bool ret = items.Remove(item);

            InventoryType type = item.getInventoryType();
            typeItems.AddOrUpdate(type, new(filterItems(type)));

            IsChanged = true;
            return ret;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public bool store(Item item)
    {
        Monitor.Enter(lockObj);
        try
        {
            if (isFull())
            { // thanks Optimist for noticing unrestricted amount of insertions here
                return false;
            }

            items.Add(item);

            InventoryType type = item.getInventoryType();
            typeItems.AddOrUpdate(type, new(filterItems(type)));

            IsChanged = true;
            return true;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public List<Item> getItems()
    {
        Monitor.Enter(lockObj);
        try
        {
            return items.ToList();
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    private List<Item> filterItems(InventoryType type)
    {
        List<Item> storageItems = getItems();
        List<Item> ret = new();

        foreach (Item item in storageItems)
        {
            if (item.getInventoryType() == type)
            {
                ret.Add(item);
            }
        }
        return ret;
    }

    public sbyte getSlot(InventoryType type, sbyte slot)
    {
        Monitor.Enter(lockObj);
        try
        {
            sbyte ret = 0;
            List<Item> storageItems = getItems();
            foreach (Item item in storageItems)
            {
                if (item == typeItems.GetValueOrDefault(type)?.get(slot))
                {
                    return ret;
                }
                ret++;
            }
            return -1;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void sendStorage(IClient c, int npcId)
    {
        if (c.OnlinedCharacter.getLevel() < 15)
        {
            c.OnlinedCharacter.dropMessage(1, "You may only use the storage once you have reached level 15.");
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        Monitor.Enter(lockObj);
        try
        {
            items.Sort((o1, o2) =>
            {
                if (o1.getInventoryType().getType() < o2.getInventoryType().getType())
                {
                    return -1;
                }
                else if (o1.getInventoryType() == o2.getInventoryType())
                {
                    return 0;
                }
                return 1;
            });

            List<Item> storageItems = getItems();
            foreach (InventoryType type in Enum.GetValues<InventoryType>())
            {
                typeItems.AddOrUpdate(type, new(storageItems));
            }

            currentNpcid = npcId;
            c.sendPacket(PacketCreator.getStorage(npcId, slots, storageItems, meso));
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void sendStored(IClient c, InventoryType type)
    {
        Monitor.Enter(lockObj);
        try
        {
            c.sendPacket(PacketCreator.storeStorage(slots, type, typeItems.GetValueOrDefault(type, new List<Item>())));
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void sendTakenOut(IClient c, InventoryType type)
    {
        Monitor.Enter(lockObj);
        try
        {
            c.sendPacket(PacketCreator.takeOutStorage(slots, type, typeItems.GetValueOrDefault(type, new List<Item>())));
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void arrangeItems(IClient c)
    {
        Monitor.Enter(lockObj);
        try
        {
            StorageInventory msi = new StorageInventory(c, items);
            msi.mergeItems();
            items = msi.sortItems();

            foreach (InventoryType type in Enum.GetValues<InventoryType>())
            {
                typeItems.AddOrUpdate(type, new(items));
            }

            c.sendPacket(PacketCreator.arrangeStorage(slots, items));
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public int getMeso()
    {
        return meso;
    }

    public void setMeso(int meso)
    {
        if (meso < 0)
        {
            throw new Exception();
        }
        this.meso = meso;
        IsChanged = true;
    }

    public void sendMeso(IClient c)
    {
        c.sendPacket(PacketCreator.mesoStorage(slots, meso));
    }

    public int getStoreFee()
    {
        // thanks to GabrielSin
        int npcId = currentNpcid;
        int? fee = trunkPutCache.get(npcId);
        if (fee == null)
        {
            fee = 100;

            DataProvider npc = DataProviderFactory.getDataProvider(WZFiles.NPC);
            var npcData = npc.getData(npcId + ".img");
            if (npcData != null)
            {
                fee = DataTool.getIntConvert("info/trunkPut", npcData, 100);
            }

            trunkPutCache.AddOrUpdate(npcId, fee.Value);
        }

        return fee.Value;
    }

    public int getTakeOutFee()
    {
        int npcId = currentNpcid;
        int? fee = trunkGetCache.get(npcId);
        if (fee == null)
        {
            fee = 0;

            DataProvider npc = DataProviderFactory.getDataProvider(WZFiles.NPC);
            var npcData = npc.getData(npcId + ".img");
            if (npcData != null)
            {
                fee = DataTool.getIntConvert("info/trunkGet", npcData, 0);
            }

            trunkGetCache.AddOrUpdate(npcId, fee.Value);
        }

        return fee.Value;
    }

    public bool isFull()
    {
        Monitor.Enter(lockObj);
        try
        {
            return items.Count >= slots;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void close()
    {
        Monitor.Enter(lockObj);
        try
        {
            typeItems.Clear();
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

}
