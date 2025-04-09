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


using Application.Core.Managers;
using client.inventory;
using client.inventory.manipulator;
using constants.inventory;
using Microsoft.EntityFrameworkCore;
using net.packet;
using server;
using tools;

namespace net.server.channel.handlers;


public class MTSHandler : AbstractPacketHandler
{
    public override void HandlePacket(InPacket p, IClient c)
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
                    { //put item up for sale
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
                            try
                            {
                                using var dbContext = new DBContext();
                                var count = dbContext.MtsItems.Where(x => x.Seller == c.OnlinedCharacter.getId()).Count();
                                if (count > 10)
                                {
                                    c.OnlinedCharacter.dropMessage(1, "You already have 10 items up for auction!");
                                    c.sendPacket(getMTS(1, 0, 0));
                                    c.sendPacket(PacketCreator.transferInventory(getTransfer(c.OnlinedCharacter.getId())));
                                    c.sendPacket(PacketCreator.notYetSoldInv(getNotYetSold(c.OnlinedCharacter.getId())));
                                    return;
                                }

                                var date = DateTimeOffset.Now.AddDays(7).ToString("yyyy-MM-dd");

                                if (!i.getInventoryType().Equals(InventoryType.EQUIP))
                                {
                                    Item item = i;
                                    var newModel = new MtsItem(1, invType.getType(), item.getItemId(), quantity, item.getExpiration(), item.getGiftFrom(), c.OnlinedCharacter.getId(), price, item.getOwner(), c.OnlinedCharacter.getName(), date);
                                    dbContext.MtsItems.Add(newModel);
                                }
                                else
                                {
                                    Equip equip = (Equip)i;
                                    var newModel = new MtsItem(1, invType.getType(), equip.getItemId(), quantity, equip.getExpiration(), equip.getGiftFrom(), c.OnlinedCharacter.getId(),
                                        price, equip.getUpgradeSlots(), equip.getLevel(), equip.getStr(), equip.getDex(), equip.getInt(), equip.getLuk(), equip.getHp(), equip.getMp(),
                                        equip.getWatk(), equip.getMatk(), equip.getWdef(), equip.getMdef(), equip.getAcc(), equip.getAvoid(), equip.getHands(), equip.getSpeed(), equip.getJump(),
                                        0, equip.getOwner(), c.OnlinedCharacter.getName(), date, equip.getVicious(), equip.getFlag(), equip.getItemExp(), equip.getItemLevel(), equip.getRingId());
                                    dbContext.MtsItems.Add(newModel);
                                }
                                dbContext.SaveChanges();
                                InventoryManipulator.removeFromSlot(c, invType, slot, quantity, false);

                            }
                            catch (Exception e)
                            {
                                log.Error(e.ToString());
                            }
                            c.OnlinedCharacter.gainMeso(-5000, false);
                            c.sendPacket(PacketCreator.MTSConfirmSell());
                            c.sendPacket(getMTS(1, 0, 0));
                            c.enableCSActions();
                            c.sendPacket(PacketCreator.transferInventory(getTransfer(c.OnlinedCharacter.getId())));
                            c.sendPacket(PacketCreator.notYetSoldInv(getNotYetSold(c.OnlinedCharacter.getId())));
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
                    { //change page
                        int tab = p.readInt();
                        int type = p.readInt();
                        int page = p.readInt();
                        c.OnlinedCharacter.changePage(page);
                        if (tab == 4 && type == 0)
                        {
                            c.sendPacket(getCart(c.OnlinedCharacter.getId()));
                        }
                        else if (tab == c.OnlinedCharacter.getCurrentTab() && type == c.OnlinedCharacter.getCurrentType() && c.OnlinedCharacter.getSearch() != null)
                        {
                            c.sendPacket(getMTSSearch(tab, type, c.OnlinedCharacter.getCurrentCI(), c.OnlinedCharacter.getSearch()!, page));
                        }
                        else
                        {
                            c.OnlinedCharacter.setSearch(null);
                            c.sendPacket(getMTS(tab, type, page));
                        }
                        c.OnlinedCharacter.changeTab(tab);
                        c.OnlinedCharacter.changeType(type);
                        c.enableCSActions();
                        c.sendPacket(PacketCreator.transferInventory(getTransfer(c.OnlinedCharacter.getId())));
                        c.sendPacket(PacketCreator.notYetSoldInv(getNotYetSold(c.OnlinedCharacter.getId())));
                        break;
                    }
                case 6:
                    {
                        //search
                        int tab = p.readInt();
                        int type = p.readInt();
                        p.readInt();
                        int ci = p.readInt();
                        string search = p.readString();
                        c.OnlinedCharacter.setSearch(search);
                        c.OnlinedCharacter.changeTab(tab);
                        c.OnlinedCharacter.changeType(type);
                        c.OnlinedCharacter.changeCI(ci);
                        c.enableCSActions();
                        c.sendPacket(PacketCreator.enableActions());
                        c.sendPacket(getMTSSearch(tab, type, ci, search, c.OnlinedCharacter.getCurrentPage()));
                        c.sendPacket(PacketCreator.showMTSCash(c.OnlinedCharacter));
                        c.sendPacket(PacketCreator.transferInventory(getTransfer(c.OnlinedCharacter.getId())));
                        c.sendPacket(PacketCreator.notYetSoldInv(getNotYetSold(c.OnlinedCharacter.getId())));
                        break;
                    }
                case 7:
                    {
                        //cancel sale
                        int id = p.readInt(); // id of the item
                        MTSManager.CancelMtsSale(id, c.OnlinedCharacter);
                        c.enableCSActions();
                        c.sendPacket(getMTS(c.OnlinedCharacter.getCurrentTab(), c.OnlinedCharacter.getCurrentType(),
                                c.OnlinedCharacter.getCurrentPage()));
                        c.sendPacket(PacketCreator.notYetSoldInv(getNotYetSold(c.OnlinedCharacter.getId())));
                        c.sendPacket(PacketCreator.transferInventory(getTransfer(c.OnlinedCharacter.getId())));
                        break;
                    }
                case 8:
                    {
                        // transfer item from transfer inv.
                        int id = p.readInt(); // id of the item
                        try
                        {
                            using var dbContext = new DBContext();
                            var dbModel = dbContext.MtsItems.Where(x => x.Id == id && x.Seller == c.OnlinedCharacter.getId() && x.Transfer == 1).OrderByDescending(x => x.Id).FirstOrDefault();
                            if (dbModel != null)
                            {
                                Item i;
                                if (dbModel.Type != 1)
                                {
                                    Item ii = new Item(dbModel.Itemid, 0, (short)dbModel.Quantity);
                                    ii.setOwner(dbModel.Owner);
                                    ii.setPosition(
                                                c.OnlinedCharacter.getInventory(ItemConstants.getInventoryType(dbModel.Itemid))
                                                        .getNextFreeSlot());
                                    i = ii.copy();
                                }
                                else
                                {
                                    Equip equip = new Equip(dbModel.Itemid, (byte)dbModel.Position, -1);
                                    equip.SetDataFromDB(dbModel);
                                    equip.setPosition(
                                                c.OnlinedCharacter.getInventory(ItemConstants.getInventoryType(dbModel.Itemid))
                                                        .getNextFreeSlot());
                                    i = equip.copy();
                                }
                                dbContext.MtsItems.Where(x => x.Id == id && x.Seller == c.OnlinedCharacter.getId() && x.Transfer == 1).ExecuteDelete();
                                InventoryManipulator.addFromDrop(c, i, false);
                                c.enableCSActions();
                                c.sendPacket(getCart(c.OnlinedCharacter.getId()));
                                c.sendPacket(getMTS(c.OnlinedCharacter.getCurrentTab(), c.OnlinedCharacter.getCurrentType(),
                                        c.OnlinedCharacter.getCurrentPage()));
                                c.sendPacket(PacketCreator.MTSConfirmTransfer(i.getQuantity(), i.getPosition()));
                                c.sendPacket(PacketCreator.transferInventory(getTransfer(c.OnlinedCharacter.getId())));
                            }

                        }
                        catch (Exception e)
                        {
                            log.Error(e, "MTS Transfer error");
                        }
                        break;
                    }
                case 9:
                    {
                        //add to cart
                        int id = p.readInt(); // id of the item
                        MTSManager.AddToCart(id, c.OnlinedCharacter);
                        c.sendPacket(getMTS(c.OnlinedCharacter.getCurrentTab(), c.OnlinedCharacter.getCurrentType(), c.OnlinedCharacter.getCurrentPage()));
                        c.enableCSActions();
                        c.sendPacket(PacketCreator.enableActions());
                        c.sendPacket(PacketCreator.transferInventory(getTransfer(c.OnlinedCharacter.getId())));
                        c.sendPacket(PacketCreator.notYetSoldInv(getNotYetSold(c.OnlinedCharacter.getId())));
                        break;
                    }
                case 10:
                    {
                        //delete from cart
                        int id = p.readInt(); // id of the item
                        MTSManager.DeleteCart(id, c.OnlinedCharacter);
                        c.sendPacket(getCart(c.OnlinedCharacter.getId()));
                        c.enableCSActions();
                        c.sendPacket(PacketCreator.transferInventory(getTransfer(c.OnlinedCharacter.getId())));
                        c.sendPacket(PacketCreator.notYetSoldInv(getNotYetSold(c.OnlinedCharacter.getId())));
                        break;
                    }
                case 12: //put item up for auction
                    break;
                case 13: //cancel wanted cart thing
                    break;
                case 14: //buy auction item now
                    break;
                case 16:
                    { //buy
                        int id = p.readInt(); // id of the item
                        try
                        {
                            using var dbContext = new DBContext();
                            var dbModel = dbContext.MtsItems.Where(x => x.Id == id).OrderByDescending(x => x.Id).FirstOrDefault();
                            if (dbModel != null)
                            {
                                int price = dbModel.Price + 100 + (int)(dbModel.Price * 0.1); // taxes
                                if (c.OnlinedCharacter.getCashShop().getCash(CashShop.NX_PREPAID) >= price)
                                { // FIX
                                    bool alwaysnull = true;
                                    foreach (var cserv in Server.getInstance().getAllChannels())
                                    {
                                        var victim = cserv.getPlayerStorage().getCharacterById(dbModel.Seller);
                                        if (victim != null)
                                        {
                                            victim.getCashShop().gainCash(4, dbModel.Price);
                                            alwaysnull = false;
                                        }
                                    }
                                    if (alwaysnull)
                                    {
                                        var accountIdData = dbContext.Characters.Where(x => x.Id == dbModel.Seller).Select(x => new { x.AccountId }).FirstOrDefault();
                                        if (accountIdData != null)
                                        {
                                            dbContext.Accounts.Where(x => x.Id == accountIdData.AccountId).ExecuteUpdate(x => x.SetProperty(y => y.NxPrepaid, y => y.NxPrepaid + dbModel.Price));
                                        }


                                        dbContext.MtsItems.Where(x => x.Id == id).ExecuteUpdate(x => x.SetProperty(y => y.Seller, c.OnlinedCharacter.getId()).SetProperty(y => y.Transfer, 1));
                                        dbContext.MtsCarts.Where(x => x.Itemid == id).ExecuteDelete();
                                        c.OnlinedCharacter.getCashShop().gainCash(4, -price);
                                        c.enableCSActions();
                                        c.sendPacket(getMTS(c.OnlinedCharacter.getCurrentTab(), c.OnlinedCharacter.getCurrentType(), c.OnlinedCharacter.getCurrentPage()));
                                        c.sendPacket(PacketCreator.MTSConfirmBuy());
                                        c.sendPacket(PacketCreator.showMTSCash(c.OnlinedCharacter));
                                        c.sendPacket(PacketCreator.transferInventory(getTransfer(c.OnlinedCharacter.getId())));
                                        c.sendPacket(PacketCreator.notYetSoldInv(getNotYetSold(c.OnlinedCharacter.getId())));
                                        c.sendPacket(PacketCreator.enableActions());
                                    }
                                    else
                                    {
                                        c.sendPacket(PacketCreator.MTSFailBuy());
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.Error(e.ToString());
                            c.sendPacket(PacketCreator.MTSFailBuy());
                        }
                        break;
                    }
                case 17:
                    { //buy from cart
                        int id = p.readInt(); // id of the item
                        try
                        {
                            using var dbContext = new DBContext();
                            var dbModel = dbContext.MtsItems.Where(x => x.Id == id).OrderByDescending(x => x.Id).FirstOrDefault();
                            if (dbModel != null)
                            {
                                int price = dbModel.Price + 100 + (int)(dbModel.Price * 0.1);
                                if (c.OnlinedCharacter.getCashShop().getCash(CashShop.NX_PREPAID) >= price)
                                {
                                    foreach (var cserv in Server.getInstance().getAllChannels())
                                    {
                                        var victim = cserv.getPlayerStorage().getCharacterById(dbModel.Seller);
                                        if (victim != null)
                                        {
                                            victim.getCashShop().gainCash(CashShop.NX_PREPAID, dbModel.Price);
                                        }
                                        else
                                        {

                                            var accountIdData = dbContext.Characters.Where(x => x.Id == dbModel.Seller).Select(x => new { x.AccountId }).FirstOrDefault();
                                            if (accountIdData != null)
                                            {
                                                dbContext.Accounts.Where(x => x.Id == accountIdData.AccountId).ExecuteUpdate(x => x.SetProperty(y => y.NxPrepaid, y => y.NxPrepaid + dbModel.Price));
                                            }
                                            dbContext.MtsItems.Where(x => x.Id == id).ExecuteUpdate(x => x.SetProperty(y => y.Seller, c.OnlinedCharacter.getId()).SetProperty(y => y.Transfer, 1));
                                            dbContext.MtsCarts.Where(x => x.Itemid == id).ExecuteDelete();

                                            c.OnlinedCharacter.getCashShop().gainCash(4, -price);
                                            c.sendPacket(getCart(c.OnlinedCharacter.getId()));
                                            c.enableCSActions();
                                            c.sendPacket(PacketCreator.MTSConfirmBuy());
                                            c.sendPacket(PacketCreator.showMTSCash(c.OnlinedCharacter));
                                            c.sendPacket(PacketCreator.transferInventory(getTransfer(c.OnlinedCharacter.getId())));
                                            c.sendPacket(PacketCreator.notYetSoldInv(getNotYetSold(c.OnlinedCharacter.getId())));
                                        }
                                    }
                                }
                                else
                                {
                                    c.sendPacket(PacketCreator.MTSFailBuy());
                                }
                            }

                        }
                        catch (Exception e)
                        {
                            log.Error(e.ToString());
                            c.sendPacket(PacketCreator.MTSFailBuy());
                        }
                        break;
                    }
                default:
                    log.Warning("Unhandled OP (MTS): {OP}, packet: {Packet}", op, p);
                    break;
            }
        }
        else
        {
            c.sendPacket(PacketCreator.showMTSCash(c.OnlinedCharacter));
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
            log.Error(e.ToString());
        }
        return items;
    }

    public Packet getCart(int cid)
    {
        List<MTSItemInfo> items = new();
        int pages = 0;
        try
        {
            using var dbContext = new DBContext();
            var dataList = dbContext.MtsCarts.Where(x => x.Cid == cid).OrderByDescending(x => x.Id).ToList();
            var itemIdList = dataList.Select(x => x.Itemid).ToList();
            var itemList = dbContext.MtsItems.Where(x => itemIdList.Contains(x.Id)).ToList();
            foreach (var rs in dataList)
            {
                var rse = itemList.FirstOrDefault(x => x.Id == rs.Itemid);
                if (rse == null)
                    continue;
                items.Add(MTSItemInfo.Map(rse));

            }

            var cartCount = dbContext.MtsCarts.Where(x => x.Cid == cid).Count();
            pages = cartCount / 16;
            if (cartCount % 16 > 0)
            {
                pages += 1;
            }
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
        return PacketCreator.sendMTS(items, 4, 0, 0, pages);
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
            log.Error(e.ToString());
        }
        return items;
    }

    private Packet getMTS(int tab, int type, int page)
    {
        List<MTSItemInfo> items = new();
        int pages = 0;
        try
        {
            using var dbContext = new DBContext();
            var query = dbContext.MtsItems.Where(x => x.Tab == tab && (type == 0 || type == x.Type) && x.Transfer == 0);
            var dataList = query
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * 16)
                .Take(16)
                .ToList()
                .Select(MTSItemInfo.Map)
                .ToList();

            var count = query.Count();
            pages = count / 16;
            if (count % 16 > 0)
            {
                pages++;
            }
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
        return PacketCreator.sendMTS(items, tab, type, page, pages); // resniff
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
                q = q.Where(x => EF.Functions.Like(x.Sellername, $"%{search}%"));
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
            log.Error(e.ToString());
        }
        return PacketCreator.sendMTS(items, tab, type, page, pages);
    }
}
