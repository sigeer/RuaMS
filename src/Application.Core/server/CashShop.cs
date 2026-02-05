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


using Application.Core.Channel;
using client.inventory;
using System.Diagnostics;
using static Application.Core.Channel.Internal.Handlers.PLifeHandlers;

namespace server;

/*
 * @author Flav
 * @author Ponk
 */
public class CashShop
{
    /// <summary>
    /// 点券
    /// </summary>
    public const int NX_CREDIT = 1;
    /// <summary>
    /// 抵用券
    /// </summary>
    public const int MAPLE_POINT = 2;
    /// <summary>
    /// 信用卡
    /// </summary>
    public const int NX_PREPAID = 4;


    private int accountId;
    private int characterId;

    private bool opened;
    private List<Item> inventory = new();
    private List<int> wishList = new();
    public int Notes { get; set; }

    public ItemType Factory { get; }
    public Player Owner { get; set; }
    public int NxCredit { get; set; }
    public int MaplePoint { get; set; }
    public int NxPrepaid { get; set; }

    public CashShop(Player player)
    {
        Owner = player;

        this.accountId = player.AccountId;
        this.characterId = player.Id;

        Factory = ItemType.CashOverall;
        if (!YamlConfig.config.server.USE_JOINT_CASHSHOP_INVENTORY)
        {
            switch (player.getJobType())
            {
                case 0:
                    Factory = ItemType.CashExplorer;
                    break;
                case 1:
                    Factory = ItemType.CashCygnus;
                    break;
                case 2:
                    Factory = ItemType.CashAran;
                    break;
                default:
                    Factory = ItemType.CashOverall;
                    break;
            }
        }
    }

    public void LoadData(int nxCredit, int maplePoint, int nxPrepaid, List<int> characterWishList, List<Item> items)
    {
        NxCredit = nxCredit;
        MaplePoint = maplePoint;
        NxPrepaid = nxPrepaid;

        inventory = items;
        wishList = characterWishList;
    }

    public void UpdateValue(int nxCredit, int maplePoint, int nxPrepaid)
    {
        NxCredit = nxCredit;
        MaplePoint = maplePoint;
        NxPrepaid = nxPrepaid;
    }


    public record CashShopSurpriseResult(Item usedCashShopSurprise, Item reward)
    {
    }

    public int getCash(int type)
    {
        return (type) switch
        {
            NX_CREDIT => NxCredit,
            MAPLE_POINT => MaplePoint,
            NX_PREPAID => NxPrepaid,
            _ => 0
        };

    }

    public bool TryGainCash(int cashType, int cashValue)
    {
        Activity.Current?.AddEvent(
        new ActivityEvent(
            "GainCash",
            tags: new ActivityTagsCollection
            {
                ["CashType"] = cashType,
                ["Delta"] = cashValue,
            }));

        switch (cashType)
        {
            case NX_CREDIT:
                {
                    var newData = NxCredit + cashValue;
                    if (newData < 0 || newData > Limits.MaxCash)
                        return false;
                    NxCredit = newData;
                    return true;
                }
            case MAPLE_POINT:
                {
                    var newData = MaplePoint + cashValue;
                    if (newData < 0 || newData > Limits.MaxCash)
                        return false;
                    MaplePoint = newData;
                    return true;
                }
            case NX_PREPAID:
                {
                    var newData = NxPrepaid + cashValue;
                    if (newData < 0 || newData > Limits.MaxCash)
                        return false;
                    NxPrepaid = newData;
                    return true;
                }
            default:
                return false;
        }
    }

    public bool BuyCashItem(int type, CashItem? buyItem)
    {
        if (buyItem == null || !buyItem.isOnSale())
            return false;


        if (!TryGainCash(type, -buyItem.getPrice()))
            return false;

        Log.Logger.Debug("Chr {CharacterName} bought cash item {ItemName} (SN {ItemSN}) for {ItemPrice}",
                Owner,
                ClientCulture.SystemCulture.GetItemName(buyItem.getItemId()),
                buyItem.getSN(),
                buyItem.getPrice());
        return true;
    }

    public void gainCash(int type, int cash)
    {
        TryGainCash(type, cash);
    }

    public void Buy(int type, CashItem? buyItem)
    {
        if (buyItem == null)
            return;

        gainCash(type, -buyItem.getPrice());
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
        return inventory.ToList();
    }

    public Item? findByCashId(long cashId)
    {
        foreach (var item in getInventory())
        {
            if (item.getCashId() == cashId)
            {
                return item;
            }
        }

        return null;
    }

    public void addToInventory(Item item)
    {
        inventory.Add(item);
    }

    public void removeFromInventory(Item item)
    {
        inventory.Remove(item);
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


    public int getAvailableNotes()
    {
        return Notes;
    }

    public void decreaseNotes()
    {
        Notes--;
    }

    public Item? getItemByCashId(long cashId)
    {
        return inventory.FirstOrDefault(x => x.getCashId() == cashId);
    }

    public int getItemsSize()
    {
        return inventory.Count;
    }
}
