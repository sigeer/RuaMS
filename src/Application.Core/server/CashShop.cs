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


using Application.Core.Managers;
using client.inventory;
using constants.id;
using constants.inventory;
using Microsoft.EntityFrameworkCore;
using net.server;

namespace server;

/*
 * @author Flav
 * @author Ponk
 */
public class CashShop
{
    ILogger? _log;
    ILogger log => _log ?? (_log = LogFactory.GetCharacterLog(accountId, characterId, "CashShop"));

    public const int NX_CREDIT = 1;
    public const int MAPLE_POINT = 2;
    public const int NX_PREPAID = 4;

    private int accountId;
    private int characterId;

    private bool opened;
    private List<Item> inventory = new();
    private List<int> wishList = new();
    private int notes = 0;
    private object lockObj = new object();
    public ItemFactory Factory { get; }
    public IPlayer Owner { get; set; }

    public CashShop(IPlayer player)
    {
        Owner = player;

        this.accountId = player.AccountId;
        this.characterId = player.Id;

        if (!YamlConfig.config.server.USE_JOINT_CASHSHOP_INVENTORY)
        {
            switch (player.getJobType())
            {
                case 0:
                    Factory = ItemFactory.CASH_EXPLORER;
                    break;
                case 1:
                    Factory = ItemFactory.CASH_CYGNUS;
                    break;
                case 2:
                    Factory = ItemFactory.CASH_ARAN;
                    break;
                default:
                    Factory = ItemFactory.CASH_OVERALL;
                    break;
            }
        }
        else
        {
            Factory = ItemFactory.CASH_OVERALL;
        }
    }

    public void LoadData(List<int> characterWishList, List<Item> items)
    {
        inventory = items.Where(x => x.getItemType() == Factory.getValue()).ToList();
        wishList = characterWishList;
    }


    public class CashItem
    {

        private int sn;
        private int itemId;
        private int price;
        private long period;
        private short count;
        private bool onSale;

        public CashItem(int sn, int itemId, int price, long period, short count, bool onSale)
        {
            this.sn = sn;
            this.itemId = itemId;
            this.price = price;
            this.period = (period == 0 ? 90 : period);
            this.count = count;
            this.onSale = onSale;
        }

        public int getSN()
        {
            return sn;
        }

        public int getItemId()
        {
            return itemId;
        }

        public int getPrice()
        {
            return price;
        }

        public short getCount()
        {
            return count;
        }

        public bool isOnSale()
        {
            return onSale;
        }

        public Item toItem()
        {
            Item item;

            int petid = -1;
            if (ItemConstants.isPet(itemId))
            {
                petid = ItemManager.CreatePet(itemId);
            }

            if (ItemConstants.getInventoryType(itemId).Equals(InventoryType.EQUIP))
            {
                item = ItemInformationProvider.getInstance().getEquipById(itemId);
            }
            else
            {
                item = new Item(itemId, 0, count, petid);
            }

            if (ItemConstants.EXPIRING_ITEMS)
            {
                if (period == 1)
                {
                    switch (itemId)
                    {
                        case ItemId.DROP_COUPON_2X_4H:
                        case ItemId.EXP_COUPON_2X_4H: // 4 Hour 2X coupons, the period is 1, but we don't want them to last a day.
                            item.setExpiration(Server.getInstance().getCurrentTime() + (long)TimeSpan.FromHours(4).TotalMilliseconds);
                            /*
                            } else if(itemId == 5211047 || itemId == 5360014) { // 3 Hour 2X coupons, unused as of now
                                    item.setExpiration(Server.getInstance().getCurrentTime() + HOURS.toMillis(3));
                            */
                            break;
                        case ItemId.EXP_COUPON_3X_2H:
                            item.setExpiration(Server.getInstance().getCurrentTime() + (long)TimeSpan.FromHours(2).TotalMilliseconds);
                            break;
                        default:
                            item.setExpiration(Server.getInstance().getCurrentTime() + (long)TimeSpan.FromDays(1).TotalMilliseconds);
                            break;
                    }
                }
                else
                {
                    item.setExpiration(Server.getInstance().getCurrentTime() + (long)TimeSpan.FromDays(period).TotalMilliseconds);
                }
            }

            item.setSN(sn);
            return item;
        }
    }

    public class SpecialCashItem
    {
        private int sn;
        private int modifier;
        private byte info; //?

        public SpecialCashItem(int sn, int modifier, byte info)
        {
            this.sn = sn;
            this.modifier = modifier;
            this.info = info;
        }

        public int getSN()
        {
            return sn;
        }

        public int getModifier()
        {
            return modifier;
        }

        public byte getInfo()
        {
            return info;
        }
    }

    public class CashItemFactory
    {
        private static volatile Dictionary<int, CashItem> items = new();
        private static volatile Dictionary<int, List<int>> packages = new();
        private static volatile List<SpecialCashItem> specialcashitems = new();

        public static void loadAllCashItems()
        {
            DataProvider etc = DataProviderFactory.getDataProvider(WZFiles.ETC);

            Dictionary<int, CashItem> loadedItems = new();
            var itemsRes = etc.getData("Commodity.img").getChildren();
            foreach (Data item in itemsRes)
            {
                int sn = DataTool.getIntConvert("SN", item);
                int itemId = DataTool.getIntConvert("ItemId", item);
                int price = DataTool.getIntConvert("Price", item, 0);
                long period = DataTool.getIntConvert("Period", item, 1);
                short count = (short)DataTool.getIntConvert("Count", item, 1);
                bool onSale = DataTool.getIntConvert("OnSale", item, 0) == 1;
                loadedItems.AddOrUpdate(sn, new CashItem(sn, itemId, price, period, count, onSale));
            }
            CashItemFactory.items = loadedItems;

            Dictionary<int, List<int>> loadedPackages = new();
            foreach (Data cashPackage in etc.getData("CashPackage.img").getChildren())
            {
                List<int> cPackage = new();

                foreach (Data item in cashPackage.getChildByPath("SN").getChildren())
                {
                    cPackage.Add(int.Parse(item.getData().ToString()));
                }

                loadedPackages.AddOrUpdate(int.Parse(cashPackage.getName()), cPackage);
            }
            CashItemFactory.packages = loadedPackages;

            try
            {
                using var dbContext = new DBContext();
                specialcashitems = dbContext.Specialcashitems.AsNoTracking().ToList()
                    .Select(x => new SpecialCashItem(x.Sn, x.Modifier, (byte)x.Info)).ToList();

            }
            catch (Exception ex)
            {
                LogFactory.GetLogger(LogType.ItemData).Error(ex.ToString());
            }
        }

        public static CashItem? getRandomCashItem()
        {
            if (items.Count == 0)
            {
                return null;
            }

            var list = items.Values.Where(x => x.isOnSale() && !ItemId.isCashPackage(x.getItemId())).ToList();
            return Randomizer.Select(list);
        }

        public static CashItem? getItem(int sn)
        {
            return items.GetValueOrDefault(sn);
        }

        public static CashItem GetItemTrust(int sn) => getItem(sn) ?? throw new BusinessResException($"getItem({sn})");

        public static List<Item> getPackage(int itemId)
        {
            return (packages.GetValueOrDefault(itemId) ?? []).Select(x => GetItemTrust(x).toItem()).ToList();
        }

        public static bool isPackage(int itemId)
        {
            return packages.ContainsKey(itemId);
        }

        public static List<SpecialCashItem> getSpecialCashItems()
        {
            return specialcashitems;
        }
    }

    public record CashShopSurpriseResult(Item usedCashShopSurprise, Item reward)
    {
    }

    public int getCash(int type)
    {
        return (type) switch
        {
            NX_CREDIT => Owner.Client.AccountEntity.NxCredit ?? 0,
            MAPLE_POINT => Owner.Client.AccountEntity.MaplePoint ?? 0,
            NX_PREPAID => Owner.Client.AccountEntity.NxPrepaid ?? 0,
            _ => 0
        };

    }

    public void gainCash(int type, int cash)
    {
        switch (type)
        {
            case NX_CREDIT:
                Owner.Client.AccountEntity.NxCredit += cash;
                break;
            case MAPLE_POINT:
                Owner.Client.AccountEntity.MaplePoint += cash;
                break;
            case NX_PREPAID:
                Owner.Client.AccountEntity.NxPrepaid += cash;
                break;
        }
    }

    public void gainCash(int type, CashItem? buyItem, int world)
    {
        if (buyItem == null)
            return;

        gainCash(type, -buyItem.getPrice());
        if (!YamlConfig.config.server.USE_ENFORCE_ITEM_SUGGESTION)
        {
            Server.getInstance().getWorld(world).addCashItemBought(buyItem.getSN());
        }
    }

    public bool isOpened()
    {
        return opened;
    }

    public void open(bool b)
    {
        opened = b;
    }

    public List<Item> getInventory()
    {
        Monitor.Enter(lockObj);
        try
        {
            return inventory.ToList();
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public Item? findByCashId(int cashId)
    {
        bool isRing;
        Equip? equip = null;
        foreach (Item item in getInventory())
        {
            if (item.getInventoryType().Equals(InventoryType.EQUIP))
            {
                equip = (Equip)item;
                isRing = equip.getRingId() > -1;
            }
            else
            {
                isRing = false;
            }

            if ((item.getPetId() > -1 ? item.getPetId() : isRing ? equip!.getRingId() : item.getCashId()) == cashId)
            {
                return item;
            }
        }

        return null;
    }

    public void addToInventory(Item item)
    {
        Monitor.Enter(lockObj);
        try
        {
            inventory.Add(item);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void removeFromInventory(Item item)
    {
        Monitor.Enter(lockObj);
        try
        {
            inventory.Remove(item);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public List<int> getWishList()
    {
        return wishList;
    }

    public void clearWishList()
    {
        wishList.Clear();
    }

    public void addToWishList(int sn)
    {
        wishList.Add(sn);
    }

    public void gift(int recipient, string from, string message, int sn, int ringid = -1)
    {
        try
        {
            var giftModel = new Gift(recipient, from, message, sn, ringid);
            using var dbContext = new DBContext();
            dbContext.Gifts.Add(giftModel);
            dbContext.SaveChanges();
        }
        catch (Exception sqle)
        {
            log.Error(sqle.ToString());
        }
    }

    public List<ItemMessagePair> loadGifts()
    {
        List<ItemMessagePair> gifts = new();
        try
        {
            using var dbContext = new DBContext();


            var dataList = dbContext.Gifts.Where(x => x.To == characterId).ToList();
            foreach (var rs in dataList)
            {
                notes++;
                var cItem = CashItemFactory.GetItemTrust(rs.Sn);
                Item item = cItem.toItem();
                Equip? equip = null;
                item.setGiftFrom(rs.From);
                if (item.getInventoryType().Equals(InventoryType.EQUIP))
                {
                    equip = (Equip)item;
                    equip.setRingId(rs.Ringid);
                    gifts.Add(new ItemMessagePair(equip, rs.Message));
                }
                else
                {
                    gifts.Add(new(item, rs.Message));
                }

                if (CashItemFactory.isPackage(cItem.getItemId()))
                {
                    //Packages never contains a ring
                    foreach (Item packageItem in CashItemFactory.getPackage(cItem.getItemId()))
                    {
                        packageItem.setGiftFrom(rs.From);
                        addToInventory(packageItem);
                    }
                }
                else
                {
                    addToInventory(equip == null ? item : equip);
                }
            }



            dbContext.Gifts.RemoveRange(dataList);
            dbContext.SaveChanges();
        }
        catch (Exception sqle)
        {
            log.Error(sqle.ToString());
        }

        return gifts;
    }

    public int getAvailableNotes()
    {
        return notes;
    }

    public void decreaseNotes()
    {
        notes--;
    }

    public void save(DBContext dbContext)
    {
        dbContext.Accounts.Where(x => x.Id == accountId).ExecuteUpdate(x =>
                x.SetProperty(y => y.NxCredit, Owner.Client.AccountEntity.NxCredit)
                .SetProperty(y => y.MaplePoint, Owner.Client.AccountEntity.MaplePoint)
                .SetProperty(y => y.NxPrepaid, Owner.Client.AccountEntity.NxPrepaid)
                );

        List<ItemInventoryType> itemsWithType = new();

        List<Item> inv = getInventory();
        foreach (Item item in inv)
        {
            itemsWithType.Add(new(item, item.getInventoryType()));
        }

        Factory.saveItems(itemsWithType, accountId, dbContext);

        dbContext.Wishlists.Where(x => x.CharId == characterId).ExecuteDelete();

        dbContext.Wishlists.AddRange(wishList.Select(x => new WishlistEntity(characterId, x)));
        dbContext.SaveChanges();
    }

    public CashShopSurpriseResult? openCashShopSurprise(long cashId)
    {
        Monitor.Enter(lockObj);
        try
        {
            Item? maybeCashShopSurprise = getItemByCashId(cashId);
            if (maybeCashShopSurprise == null ||
                    maybeCashShopSurprise.getItemId() != ItemId.CASH_SHOP_SURPRISE)
            {
                return null;
            }

            Item cashShopSurprise = maybeCashShopSurprise;
            if (cashShopSurprise.getQuantity() <= 0)
            {
                return null;
            }

            if (getItemsSize() >= 100)
            {
                return null;
            }

            var cashItemReward = CashItemFactory.getRandomCashItem();
            if (cashItemReward == null)
            {
                return null;
            }

            short newQuantity = (short)(cashShopSurprise.getQuantity() - 1);
            cashShopSurprise.setQuantity(newQuantity);
            if (newQuantity <= 0)
            {
                removeFromInventory(cashShopSurprise);
            }

            Item itemReward = cashItemReward.toItem();
            addToInventory(itemReward);

            return new CashShopSurpriseResult(cashShopSurprise, itemReward);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }


    private Item? getItemByCashId(long cashId)
    {
        return inventory.FirstOrDefault(x => x.getCashId() == cashId);
    }

    public int getItemsSize()
    {
        Monitor.Enter(lockObj);
        try
        {
            return inventory.Count;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public static Item generateCouponItem(int itemId, short quantity)
    {
        CashItem it = new CashItem(77777777, itemId, 7777, ItemConstants.isPet(itemId) ? 30 : 0, quantity, true);
        return it.toItem();
    }
}
