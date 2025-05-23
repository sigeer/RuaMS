/*
	This file is part of the OdinMS Maple Story NewServer
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

    Copyleft (L) 2016 - 2019 RonanLana (HeavenMS)

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


using Application.Core.Game.Players;
using Application.EF;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using net.packet;
using server;
using tools;

namespace Application.Core.Channel.Net.Handlers;


/**
 * @author Penguins (Acrylic)
 * @author Ronan (HeavenMS)
 */
public class CouponCodeHandler : ChannelHandlerBase
{
    readonly ILogger<CouponCodeHandler> _logger;

    public CouponCodeHandler(ILogger<CouponCodeHandler> logger)
    {
        _logger = logger;
    }

    private List<TypedItemQuantity> getNXCodeItems(IPlayer chr, DBContext dbContext, int codeid)
    {
        Dictionary<int, int> couponItems = new();
        Dictionary<int, int> couponPoints = new(5);

        var dataList = dbContext.NxcodeItems.AsNoTracking().Where(x => x.Codeid == codeid).ToList();
        foreach (var rs in dataList)
        {
            int type = rs.Type, quantity = rs.Quantity;

            if (type < 5)
                couponPoints[type] = couponPoints.GetValueOrDefault(type) + quantity;
            else
                couponItems[type] = couponItems.GetValueOrDefault(rs.Item) + quantity;
        }

        List<TypedItemQuantity> ret = new();
        if (couponItems.Count > 0)
        {
            foreach (var e in couponItems)
            {
                int item = e.Key, qty = e.Value;

                if (ItemInformationProvider.getInstance().getName(item) == null)
                {
                    item = 4000000;
                    qty = 1;

                    _logger.LogWarning("Error trying to redeem itemid {ItemId} from coupon codeid {ItemCodeId}", item, codeid);
                }

                if (!chr.canHold(item, qty))
                {
                    return [];
                }

                ret.Add(new(5, new(item, qty)));
            }
        }

        if (couponPoints.Count > 0)
        {
            foreach (var e in couponPoints)
            {
                ret.Add(new(e.Key, new(777, e.Value)));
            }
        }

        return ret;
    }

    private StatuedTypedItemQuantity getNXCodeResult(IPlayer chr, string code)
    {
        var c = chr.Client;
        List<TypedItemQuantity> ret = new List<TypedItemQuantity>();
        try
        {
            if (!c.attemptCsCoupon())
            {
                return new StatuedTypedItemQuantity(-5, []);
            }

            using var dbContext = new DBContext();
            var dbModel = dbContext.Nxcodes.Where(x => x.Code == code).FirstOrDefault();
            if (dbModel == null)
            {
                return new StatuedTypedItemQuantity(-1, []);
            }

            if (dbModel.Retriever != null)
            {
                return new StatuedTypedItemQuantity(-2, []);
            }

            if (dbModel.Expiration < c.CurrentServer.getCurrentTime())
            {
                return new StatuedTypedItemQuantity(-3, []);
            }

            int codeid = dbModel.Id;

            ret = getNXCodeItems(chr, dbContext, codeid);
            if (ret == null)
            {
                return new StatuedTypedItemQuantity(-4, []);
            }

            dbModel.Retriever = chr.getName();
            dbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }

        c.resetCsCoupon();
        return new StatuedTypedItemQuantity(0, ret);
    }


    private static int parseCouponResult(int res)
    {
        switch (res)
        {
            case -1:
                return 0xB0;

            case -2:
                return 0xB3;

            case -3:
                return 0xB2;

            case -4:
                return 0xBB;

            default:
                return 0xB1;
        }
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        p.skip(2);
        string code = p.readString();

        if (c.tryacquireClient())
        {
            try
            {
                var codeRes = getNXCodeResult(c.OnlinedCharacter, code.ToUpper());
                int type = codeRes.status;
                if (type < 0)
                {
                    c.sendPacket(PacketCreator.showCashShopMessage((byte)parseCouponResult(type)));
                }
                else
                {
                    List<Item> cashItems = new();
                    List<ItemQuantity> items = new();
                    int nxCredit = 0;
                    int maplePoints = 0;
                    int nxPrepaid = 0;
                    int mesos = 0;

                    foreach (var pair in codeRes.data)
                    {
                        type = pair.Type;
                        int quantity = pair.Item.Quantity;

                        CashShop cs = c.OnlinedCharacter.getCashShop();
                        switch (type)
                        {
                            case 0:
                                c.OnlinedCharacter.gainMeso(quantity, false); //mesos
                                mesos += quantity;
                                break;
                            case 4:
                                cs.gainCash(1, quantity);    //nxCredit
                                nxCredit += quantity;
                                break;
                            case 1:
                                cs.gainCash(2, quantity);    //maplePoint
                                maplePoints += quantity;
                                break;
                            case 2:
                                cs.gainCash(4, quantity);    //nxPrepaid
                                nxPrepaid += quantity;
                                break;
                            case 3:
                                cs.gainCash(1, quantity);
                                nxCredit += quantity;
                                cs.gainCash(4, (quantity / 5000));
                                nxPrepaid += quantity / 5000;
                                break;

                            default:
                                int item = pair.Item.ItemId;

                                short qty;
                                if (quantity > short.MaxValue)
                                {
                                    qty = short.MaxValue;
                                }
                                else if (quantity < short.MinValue)
                                {
                                    qty = short.MinValue;
                                }
                                else
                                {
                                    qty = (short)quantity;
                                }

                                if (ItemInformationProvider.getInstance().isCash(item))
                                {
                                    Item it = c.getChannelServer().Service.GenerateCouponItem(item, qty);

                                    cs.addToInventory(it);
                                    cashItems.Add(it);
                                }
                                else
                                {
                                    InventoryManipulator.addById(c, item, qty, "", -1);
                                    items.Add(new(item, qty));
                                }
                                break;
                        }
                    }
                    if (cashItems.Count > 255)
                    {
                        cashItems = cashItems.Take(255).ToList();
                    }
                    if (nxCredit != 0 || nxPrepaid != 0)
                    { //coupon packet can only show maple points (afaik)
                        c.sendPacket(PacketCreator.showBoughtQuestItem(0));
                    }
                    else
                    {
                        c.sendPacket(PacketCreator.showCouponRedeemedItems(c.AccountEntity!.Id, maplePoints, mesos, cashItems, items));
                    }
                    c.enableCSActions();
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}