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


using Application.Core.Servers.Services;
using Application.Core.ServerTransports;
using Application.Shared.Items;
using client.inventory;
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
    public int Notes { get; set; }
    private object lockObj = new object();
    public ItemType Factory { get; }
    public IPlayer Owner { get; set; }
    public int NxCredit { get; set; }
    public int MaplePoint { get; set; }
    public int NxPrepaid { get; set; }

    public CashShop(IPlayer player)
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

    public void gainCash(int type, int cash)
    {
        switch (type)
        {
            case NX_CREDIT:
                NxCredit += cash;
                break;
            case MAPLE_POINT:
                MaplePoint += cash;
                break;
            case NX_PREPAID:
                NxPrepaid += cash;
                break;
        }
    }

    public void Buy(int type, CashItem? buyItem)
    {
        if (buyItem == null)
            return;

        gainCash(type, -buyItem.getPrice());
        if (!YamlConfig.config.server.USE_ENFORCE_ITEM_SUGGESTION)
        {
            Owner.Client.CurrentServer.Service.AddCashItemBought(buyItem.getSN());
        }
    }

    public bool isOpened()
    {
        return opened;
    }

    public void open(bool b)
    {
        opened = b;

        if (!opened)
            Owner.saveCharToDB();
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

    public void gift(int recipient, string from, string message, int sn, long ringid = -1)
    {
        Owner.Client.CurrentServerContainer.Transport.SendGift(recipient, from, message, sn, ringid);
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
}
