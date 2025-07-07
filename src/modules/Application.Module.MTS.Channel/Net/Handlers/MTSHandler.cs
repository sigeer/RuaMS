/*
 This file is part of the OdinMS Maple Story NewServer
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


using Application.Core.Channel.DataProviders;
using Application.Core.Channel.Net;
using Application.Core.Client;
using Application.EF;
using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Item;
using Application.Shared.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Module.MTS.Channel.Net.Handlers;


public class MTSHandler : ChannelHandlerBase
{
    readonly ILogger<MTSHandler> _logger;
    readonly MTSManager _manager;

    public MTSHandler(ILogger<MTSHandler> logger, MTSManager manager)
    {
        _logger = logger;
        _manager = manager;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        // TODO add karma-to-untradeable flag on sold items here

        if (!c.OnlinedCharacter.getCashShop().isOpened())
        {
            return;
        }
        if (p.available() > 0)
        {
            byte op = p.readByte();
            switch (op)
            {
                case 2:
                    {
                        //put item up for sale
                        byte itemtype = p.readByte();
                        int itemid = p.readInt();
                        p.readShort();
                        p.skip(7);
                        short stars = 1;
                        if (itemtype == 1)
                        {
                            p.skip(32);
                        }
                        else
                        {
                            stars = p.readShort();
                        }
                        p.readString(); // another useless thing (owner)
                        if (itemtype == 1)
                        {
                            p.skip(32);
                        }
                        else
                        {
                            p.readShort();
                        }
                        short slot;
                        short quantity;
                        if (itemtype != 1)
                        {
                            if (itemid / 10000 == 207 || itemid / 10000 == 233)
                            {
                                p.skip(8);
                            }
                            slot = (short)p.readInt();
                        }
                        else
                        {
                            slot = (short)p.readInt();
                        }
                        if (itemtype != 1)
                        {
                            if (itemid / 10000 == 207 || itemid / 10000 == 233)
                            {
                                quantity = stars;
                                p.skip(4);
                            }
                            else
                            {
                                quantity = (short)p.readInt();
                            }
                        }
                        else
                        {
                            quantity = (byte)p.readInt();
                        }
                        int price = p.readInt();
                        if (itemtype == 1)
                        {
                            quantity = 1;
                        }
                        if (quantity < 0 || price < 110 || c.OnlinedCharacter.getItemQuantity(itemid, false) < quantity)
                        {
                            return;
                        }
                        InventoryType invType = ItemConstants.getInventoryType(itemid);
                        var i = c.OnlinedCharacter.getInventory(invType).getItem(slot)?.copy();
                        if (i != null && c.OnlinedCharacter.getMeso() >= 5000)
                        {
                            _manager.AddItemToSale(c.OnlinedCharacter, i, price);
                        }
                        break;
                    }
                case 3: //send offer for wanted item
                    break;
                case 4: //list wanted item
                    p.readInt();
                    p.readInt();
                    p.readInt();
                    p.readShort();
                    p.readString();
                    break;
                case 5:
                    {
                        //change page
                        int tab = p.readInt();
                        int type = p.readInt();
                        int page = p.readInt();
                        _manager.Query(c.OnlinedCharacter, tab, type, page);
                        break;
                    }
                case 6:
                    {
                        //search
                        int tab = p.readInt();
                        int type = p.readInt();
                        int unKnown = p.readInt(); // 会不会是page？
                        int ci = p.readInt();
                        string search = p.readString();
                        _manager.Search(c.OnlinedCharacter, tab, type, ci, search);
                        break;
                    }
                case 7:
                    {
                        //cancel sale
                        int id = p.readInt(); // id of the item
                        _manager.CancelSaleItem(c.OnlinedCharacter, id);
                        break;
                    }
                case 8:
                    {
                        // transfer item from transfer inv.
                        int id = p.readInt(); // id of the item
                        _manager.TakeItemFromTransferInv(c.OnlinedCharacter, id);
                        break;
                    }
                case 9:
                    {
                        //add to cart
                        int id = p.readInt(); // id of the item
                        _manager.AddCartItem(c.OnlinedCharacter, id);
                        break;
                    }
                case 10:
                    {
                        //delete from cart
                        int id = p.readInt(); // id of the item
                        _manager.RemoveCartItem(c.OnlinedCharacter, id);
                        break;
                    }
                case 12: //put item up for auction
                    break;
                case 13: //cancel wanted cart thing
                    break;
                case 14: //buy auction item now
                    break;
                case 16:
                    {
                        //buy
                        int id = p.readInt(); // id of the item
                        _manager.Buy(c.OnlinedCharacter, id, false);
                        break;
                    }
                case 17:
                    {
                        //buy from cart
                        int id = p.readInt(); // id of the item
                        _manager.Buy(c.OnlinedCharacter, id, true);
                        break;
                    }
                default:
                    _logger.LogWarning("Unhandled OP (MTS): {OP}, packet: {Packet}", op, p);
                    break;
            }
        }
        else
        {
            c.sendPacket(MTSPacketCreator.showMTSCash(c.OnlinedCharacter));
        }
    }

    public List<MTSItemInfo> getNotYetSold(int cid)
    {
        List<MTSItemInfo> items = new();
        try
        {
            using var dbContext = new DBContext();
            return dbContext.MtsItems.Where(x => x.Seller == cid && x.Transfer == 0).OrderByDescending(x => x.Id).ToList().Select(x => MTSItemInfo.Map(x)).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
        return items;
    }


    public List<MTSItemInfo> getTransfer(int cid)
    {
        List<MTSItemInfo> items = new();
        try
        {
            using var dbContext = new DBContext();
            var dataList = dbContext.MtsItems.Where(x => x.Seller == cid && x.Transfer == 1).OrderByDescending(x => x.Id).ToList().Select(MTSItemInfo.Map).ToList();

        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
        return items;
    }


    public Packet getMTSSearch(int tab, int type, int cOi, string search, int page)
    {
        List<MTSItemInfo> items = new();
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        int pages = 0;
        try
        {
            using var dbContext = new DBContext();
            var q = dbContext.MtsItems.Where(x => x.Tab == tab && x.Transfer == 0);
            if (type != 0)
                q = q.Where(x => x.Type == type);
            if (cOi != 0)
            {
                var filteredItemId = ii.getAllItems().Where(itemPair => itemPair.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).Select(x => x.Id).ToList();
                q = q.Where(x => filteredItemId.Contains(x.Itemid));
            }
            else
            {
                q = q.Where(x => Microsoft.EntityFrameworkCore.EF.Functions.Like(x.Sellername, $"%{search}%"));
            }
            items = q.OrderBy(x => x.Id).Skip((page - 1) * 16).Take(16).ToList()
                .Select(MTSItemInfo.Map)
                .ToList();

            if (type == 0)
            {
                var count = q.Count();
                pages = count / 16;
                if (count % 16 > 0)
                {
                    pages++;
                }
            }

        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
        return MTSPacketCreator.sendMTS(items, tab, type, page, pages);
    }
}
