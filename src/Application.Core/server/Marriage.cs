/*
    This file is part of the HeavenMS MapleStory Server
    Copyleft (L) 2016 - 2019 RonanLana

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
using client.inventory.manipulator;
using scripting.Event;

namespace server;
/**
 * @author Ronan
 */
public class Marriage : EventInstanceManager
{
    public Marriage(EventManager em, string name) : base(em, name)
    {

    }

    public bool giftItemToSpouse(int cid)
    {
        return this.getIntProperty("wishlistSelection") == 0;
    }

    public List<string> getWishlistItems(bool groom)
    {
        string strItems = this.getProperty(groom ? "groomWishlist" : "brideWishlist");
        if (strItems != null)
        {
            return Arrays.asList(strItems.Split("\r\n"));
        }

        return new();
    }

    public void initializeGiftItems()
    {
        List<Item> groomGifts = new();
        this.setObjectProperty("groomGiftlist", groomGifts);

        List<Item> brideGifts = new();
        this.setObjectProperty("brideGiftlist", brideGifts);
    }

    public List<Item> getGiftItems(IClient c, bool groom)
    {
        List<Item> gifts = getGiftItemsList(groom);
        lock (gifts)
        {
            return new(gifts);
        }
    }

    private List<Item> getGiftItemsList(bool groom)
    {
        return (List<Item>)this.getObjectProperty(groom ? "groomGiftlist" : "brideGiftlist");
    }

    public Item? getGiftItem(IClient c, bool groom, int idx)
    {
        try
        {
            return getGiftItems(c, groom).ElementAtOrDefault(idx);
        }
        catch (IndexOutOfRangeException e)
        {
            return null;
        }
    }

    public void addGiftItem(bool groom, Item item)
    {
        List<Item> gifts = getGiftItemsList(groom);
        lock (gifts)
        {
            gifts.Add(item);
        }
    }

    public void removeGiftItem(bool groom, Item item)
    {
        List<Item> gifts = getGiftItemsList(groom);
        lock (gifts)
        {
            gifts.Remove(item);
        }
    }

    public bool? isMarriageGroom(IPlayer chr)
    {
        bool? groom = null;
        try
        {
            int groomid = this.getIntProperty("groomId"), brideid = this.getIntProperty("brideId");
            if (chr.getId() == groomid)
            {
                groom = true;
            }
            else if (chr.getId() == brideid)
            {
                groom = false;
            }
        }
        catch (Exception nfe)
        {
        }

        return groom;
    }

    public static bool claimGiftItems(IClient c, IPlayer chr)
    {
        List<Item> gifts = loadGiftItemsFromDb(c, chr.getId());
        if (Inventory.checkSpot(chr, gifts))
        {
            try
            {
                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                ItemFactory.MARRIAGE_GIFTS.saveItems(new(), chr.getId(), dbContext);
                dbTrans.Commit();
            }
            catch (Exception sqle)
            {
                Log.Logger.Error(sqle.ToString());
            }

            foreach (Item item in gifts)
            {
                InventoryManipulator.addFromDrop(chr.getClient(), item, false);
            }

            return true;
        }

        return false;
    }

    public static List<Item> loadGiftItemsFromDb(IClient c, int cid)
    {
        List<Item> items = new();

        try
        {
            foreach (var it in ItemFactory.MARRIAGE_GIFTS.loadItems(cid, false))
            {
                items.Add(it.Item);
            }
        }
        catch (Exception sqle)
        {
            Log.Logger.Error(sqle.ToString());
        }

        return items;
    }

    public void saveGiftItemsToDb(IClient c, bool groom, int cid)
    {
        Marriage.saveGiftItemsToDb(c, getGiftItems(c, groom), cid);
    }

    public static void saveGiftItemsToDb(IClient c, List<Item> giftItems, int cid)
    {
        List<ItemInventoryType> items = new();
        foreach (Item it in giftItems)
        {
            items.Add(new(it, it.getInventoryType()));
        }

        try
        {
            using var dbContext = new DBContext();
            using var dbTrans = dbContext.Database.BeginTransaction();
            ItemFactory.MARRIAGE_GIFTS.saveItems(items, cid, dbContext);
            dbTrans.Commit();
        }
        catch (Exception sqle)
        {
            Log.Logger.Error(sqle.ToString());
        }
    }
}
