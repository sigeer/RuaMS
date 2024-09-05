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
using client.inventory.manipulator;
using constants.game;
using net.server.coordinator.world;
using System.Text;
using tools;

namespace server;


/**
 * @author Matze
 * @author Ronan - concurrency safety + check available slots + trade results
 */
public class Trade
{
    private static ILogger log = LogFactory.GetLogger("Trade");


    private Trade partner = null;
    private List<Item> items = new();
    private List<Item> exchangeItems;
    private int meso = 0;
    private int exchangeMeso;
    private AtomicBoolean locked = new AtomicBoolean(false);
    private IPlayer chr;
    private byte number;
    private bool fullTrade = false;

    public Trade(byte number, IPlayer chr)
    {
        this.chr = chr;
        this.number = number;
    }

    public static int getFee(long meso)
    {
        long fee = 0;
        if (meso >= 100000000)
        {
            fee = (meso * 6) / 100;
        }
        else if (meso >= 25000000)
        {
            fee = (meso * 5) / 100;
        }
        else if (meso >= 10000000)
        {
            fee = (meso * 4) / 100;
        }
        else if (meso >= 5000000)
        {
            fee = (meso * 3) / 100;
        }
        else if (meso >= 1000000)
        {
            fee = (meso * 18) / 1000;
        }
        else if (meso >= 100000)
        {
            fee = (meso * 8) / 1000;
        }
        return (int)fee;
    }

    private void lockTrade()
    {
        locked.Set(true);
        partner.getChr().sendPacket(PacketCreator.getTradeConfirmation());
    }

    private void fetchExchangedItems()
    {
        exchangeItems = partner.getItems();
        exchangeMeso = partner.getMeso();
    }

    private void completeTrade()
    {
        byte result;
        bool show = YamlConfig.config.server.USE_DEBUG;
        items.Clear();
        meso = 0;

        foreach (Item item in exchangeItems)
        {
            KarmaManipulator.toggleKarmaFlagToUntradeable(item);
            InventoryManipulator.addFromDrop(chr.getClient(), item, show);
        }

        if (exchangeMeso > 0)
        {
            int fee = getFee(exchangeMeso);

            chr.gainMeso(exchangeMeso - fee, show, true, show);
            if (fee > 0)
            {
                chr.dropMessage(1, "Transaction completed. You received " + GameConstants.numberWithCommas(exchangeMeso - fee) + " mesos due to trade fees.");
            }
            else
            {
                chr.dropMessage(1, "Transaction completed. You received " + GameConstants.numberWithCommas(exchangeMeso) + " mesos.");
            }

            result = (byte)TradeResult.NO_RESPONSE;
        }
        else
        {
            result = (byte)TradeResult.SUCCESSFUL;
        }

        exchangeMeso = 0;
        if (exchangeItems != null)
        {
            exchangeItems.Clear();
        }

        chr.sendPacket(PacketCreator.getTradeResult(number, result));
    }

    private void cancel(byte result)
    {
        bool show = YamlConfig.config.server.USE_DEBUG;

        foreach (Item item in items)
        {
            InventoryManipulator.addFromDrop(chr.getClient(), item, show);
        }
        if (meso > 0)
        {
            chr.gainMeso(meso, show, true, show);
        }
        meso = 0;
        if (items != null)
        {
            items.Clear();
        }
        exchangeMeso = 0;
        if (exchangeItems != null)
        {
            exchangeItems.Clear();
        }

        chr.sendPacket(PacketCreator.getTradeResult(number, result));
    }

    private bool isLocked()
    {
        return locked.Get();
    }

    private int getMeso()
    {
        return meso;
    }

    public void setMeso(int meso)
    {
        if (locked)
        {
            throw new Exception("Trade is locked.");
        }
        if (meso < 0)
        {
            log.Warning("[Hack] Chr {CharacterName} is trying to trade negative mesos", chr.getName());
            return;
        }
        if (chr.getMeso() >= meso)
        {
            chr.gainMeso(-meso, false, true, false);
            this.meso += meso;
            chr.sendPacket(PacketCreator.getTradeMesoSet(0, this.meso));
            if (partner != null)
            {
                partner.getChr().sendPacket(PacketCreator.getTradeMesoSet(1, this.meso));
            }
        }
        else
        {
        }
    }

    public bool addItem(Item item)
    {
        lock (items)
        {
            if (items.Count > 9)
            {
                return false;
            }
            foreach (Item it in items)
            {
                if (it.getPosition() == item.getPosition())
                {
                    return false;
                }
            }

            items.Add(item);
        }

        return true;
    }

    public void chat(string message)
    {
        chr.sendPacket(PacketCreator.getTradeChat(chr, message, true));
        if (partner != null)
        {
            partner.getChr().sendPacket(PacketCreator.getTradeChat(chr, message, false));
        }
    }

    public Trade getPartner()
    {
        return partner;
    }

    public void setPartner(Trade partner)
    {
        if (locked)
        {
            return;
        }
        this.partner = partner;
    }

    public IPlayer getChr()
    {
        return chr;
    }

    public List<Item> getItems()
    {
        return new(items);
    }

    public int getExchangeMesos()
    {
        return exchangeMeso;
    }

    private bool fitsMeso()
    {
        return chr.canHoldMeso(exchangeMeso - getFee(exchangeMeso));
    }

    private bool fitsInInventory()
    {
        List<ItemInventoryType> tradeItems = new();
        foreach (Item item in exchangeItems)
        {
            tradeItems.Add(new(item, item.getInventoryType()));
        }

        return Inventory.checkSpotsAndOwnership(chr, tradeItems);
    }

    private bool fitsUniquesInInventory()
    {
        List<int> exchangeItemids = new();
        foreach (Item item in exchangeItems)
        {
            exchangeItemids.Add(item.getItemId());
        }

        return chr.canHoldUniques(exchangeItemids);
    }

    object checkLock = new object();
    private bool checkTradeCompleteHandshake(bool updateSelf)
    {
        lock (checkLock)
        {
            Trade self, other;

            if (updateSelf)
            {
                self = this;
                other = this.getPartner();
            }
            else
            {
                self = this.getPartner();
                other = this;
            }

            if (self.isLocked())
            {
                return false;
            }

            self.lockTrade();
            return other.isLocked();
        }
    }

    private bool checkCompleteHandshake()
    {  // handshake checkout thanks to Ronan
        if (this.getChr().getId() < this.getPartner().getChr().getId())
        {
            return this.checkTradeCompleteHandshake(true);
        }
        else
        {
            return this.getPartner().checkTradeCompleteHandshake(false);
        }
    }

    public static void completeTrade(IPlayer chr)
    {
        var local = chr.getTrade();
        var partner = local.getPartner();
        if (local.checkCompleteHandshake())
        {
            local.fetchExchangedItems();
            partner.fetchExchangedItems();

            if (!local.fitsMeso())
            {
                cancelTrade(local.getChr(), TradeResult.UNSUCCESSFUL);
                chr.message("There is not enough meso inventory space to complete the trade.");
                partner.getChr().message("Partner does not have enough meso inventory space to complete the trade.");
                return;
            }
            else if (!partner.fitsMeso())
            {
                cancelTrade(partner.getChr(), TradeResult.UNSUCCESSFUL);
                chr.message("Partner does not have enough meso inventory space to complete the trade.");
                partner.getChr().message("There is not enough meso inventory space to complete the trade.");
                return;
            }

            if (!local.fitsInInventory())
            {
                if (local.fitsUniquesInInventory())
                {
                    cancelTrade(local.getChr(), TradeResult.UNSUCCESSFUL);
                    chr.message("There is not enough inventory space to complete the trade.");
                    partner.getChr().message("Partner does not have enough inventory space to complete the trade.");
                }
                else
                {
                    cancelTrade(local.getChr(), TradeResult.UNSUCCESSFUL_UNIQUE_ITEM_LIMIT);
                    partner.getChr().message("Partner cannot hold more than one one-of-a-kind item at a time.");
                }
                return;
            }
            else if (!partner.fitsInInventory())
            {
                if (partner.fitsUniquesInInventory())
                {
                    cancelTrade(partner.getChr(), TradeResult.UNSUCCESSFUL);
                    chr.message("Partner does not have enough inventory space to complete the trade.");
                    partner.getChr().message("There is not enough inventory space to complete the trade.");
                }
                else
                {
                    cancelTrade(partner.getChr(), TradeResult.UNSUCCESSFUL_UNIQUE_ITEM_LIMIT);
                    chr.message("Partner cannot hold more than one one-of-a-kind item at a time.");
                }
                return;
            }

            if (local.getChr().getLevel() < 15)
            {
                if (local.getChr().getMesosTraded() + local.exchangeMeso > 1000000)
                {
                    cancelTrade(local.getChr(), TradeResult.NO_RESPONSE);
                    local.getChr().sendPacket(PacketCreator.serverNotice(1, "Characters under level 15 may not trade more than 1 million mesos per day."));
                    return;
                }
                else
                {
                    local.getChr().addMesosTraded(local.exchangeMeso);
                }
            }
            else if (partner.getChr().getLevel() < 15)
            {
                if (partner.getChr().getMesosTraded() + partner.exchangeMeso > 1000000)
                {
                    cancelTrade(partner.getChr(), TradeResult.NO_RESPONSE);
                    partner.getChr().sendPacket(PacketCreator.serverNotice(1, "Characters under level 15 may not trade more than 1 million mesos per day."));
                    return;
                }
                else
                {
                    partner.getChr().addMesosTraded(partner.exchangeMeso);
                }
            }

            logTrade(local, partner);
            local.completeTrade();
            partner.completeTrade();

            partner.getChr().setTrade(null);
            chr.setTrade(null);
        }
    }

    private static void cancelTradeInternal(IPlayer chr, byte selfResult, byte partnerResult)
    {
        var trade = chr.getTrade();
        if (trade == null)
        {
            return;
        }

        trade.cancel(selfResult);
        if (trade.getPartner() != null)
        {
            trade.getPartner().cancel(partnerResult);
            trade.getPartner().getChr().setTrade(null);

            InviteCoordinator.answerInvite(InviteType.TRADE, trade.getChr().getId(), trade.getPartner().getChr().getId(), false);
            InviteCoordinator.answerInvite(InviteType.TRADE, trade.getPartner().getChr().getId(), trade.getChr().getId(), false);
        }
        chr.setTrade(null);
    }

    private static byte[] tradeResultsPair(byte result)
    {
        byte selfResult, partnerResult;

        if (result == (byte)TradeResult.PARTNER_CANCEL)
        {
            partnerResult = result;
            selfResult = (byte)TradeResult.NO_RESPONSE;
        }
        else if (result == (byte)TradeResult.UNSUCCESSFUL_UNIQUE_ITEM_LIMIT)
        {
            partnerResult = (byte)TradeResult.UNSUCCESSFUL;
            selfResult = result;
        }
        else
        {
            partnerResult = result;
            selfResult = result;
        }

        return new byte[] { selfResult, partnerResult };
    }

    object cancelLock = new object();
    private void tradeCancelHandshake(bool updateSelf, byte result)
    {
        lock (cancelLock)
        {
            byte selfResult, partnerResult;
            Trade self;

            byte[] pairedResult = tradeResultsPair(result);
            selfResult = pairedResult[0];
            partnerResult = pairedResult[1];

            if (updateSelf)
            {
                self = this;
            }
            else
            {
                self = this.getPartner();
            }

            cancelTradeInternal(self.getChr(), selfResult, partnerResult);
        }
    }

    private void cancelHandshake(byte result)
    {  // handshake checkout thanks to Ronan
        Trade partner = this.getPartner();
        if (partner == null || this.getChr().getId() < partner.getChr().getId())
        {
            this.tradeCancelHandshake(true, result);
        }
        else
        {
            partner.tradeCancelHandshake(false, result);
        }
    }

    public static void cancelTrade(IPlayer chr, TradeResult result)
    {
        var trade = chr.getTrade();
        if (trade == null)
        {
            return;
        }

        trade.cancelHandshake((byte)result);
    }

    public static void startTrade(IPlayer chr)
    {
        if (chr.getTrade() == null)
        {
            chr.setTrade(new Trade(0, chr));
        }
    }

    private static bool hasTradeInviteBack(IPlayer c1, IPlayer c2)
    {
        var other = c2.getTrade();
        if (other != null)
        {
            Trade otherPartner = other.getPartner();
            if (otherPartner != null)
            {
                return otherPartner.getChr().getId() == c1.getId();
            }
        }

        return false;
    }

    public static void inviteTrade(IPlayer c1, IPlayer c2)
    {

        if ((c1.isGM() && !c2.isGM()) && c1.gmLevel() < YamlConfig.config.server.MINIMUM_GM_LEVEL_TO_TRADE)
        {
            c1.message("You cannot trade with non-GM characters.");
            log.Information("GM {GMName} blocked from trading with {CharacterName} due to GM level.", c1.getName(), c2.getName());
            cancelTrade(c1, TradeResult.NO_RESPONSE);
            return;
        }

        if ((!c1.isGM() && c2.isGM()) && c2.gmLevel() < YamlConfig.config.server.MINIMUM_GM_LEVEL_TO_TRADE)
        {
            c1.message("You cannot trade with this GM character.");
            cancelTrade(c1, TradeResult.NO_RESPONSE);
            return;
        }

        if (InviteCoordinator.hasInvite(InviteType.TRADE, c1.getId()))
        {
            if (hasTradeInviteBack(c1, c2))
            {
                c1.message("You are already managing this player's trade invitation.");
            }
            else
            {
                c1.message("You are already managing someone's trade invitation.");
            }

            return;
        }
        else if (c1.getTrade().isFullTrade())
        {
            c1.message("You are already in a trade.");
            return;
        }

        if (InviteCoordinator.createInvite(InviteType.TRADE, c1, c1.getId(), c2.getId()))
        {
            if (c2.getTrade() == null)
            {
                var c2Trade = new Trade(1, c2);
                c2.setTrade(c2Trade);
                c2.getTrade()!.setPartner(c1.getTrade());
                c1.getTrade().setPartner(c2.getTrade());

                c1.sendPacket(PacketCreator.getTradeStart(c1.getClient(), c1.getTrade(), 0));
                c2.sendPacket(PacketCreator.tradeInvite(c1));
            }
            else
            {
                c1.message("The other player is already trading with someone else.");
                cancelTrade(c1, TradeResult.NO_RESPONSE);
                InviteCoordinator.answerInvite(InviteType.TRADE, c2.getId(), c1.getId(), false);
            }
        }
        else
        {
            c1.message("The other player is already managing someone else's trade invitation.");
            cancelTrade(c1, TradeResult.NO_RESPONSE);
        }
    }

    public static void visitTrade(IPlayer c1, IPlayer c2)
    {
        InviteResult inviteRes = InviteCoordinator.answerInvite(InviteType.TRADE, c1.getId(), c2.getId(), true);

        InviteResultType res = inviteRes.result;
        if (res == InviteResultType.ACCEPTED)
        {
            if (c1.getTrade() != null && c1.getTrade().getPartner() == c2.getTrade() && c2.getTrade() != null && c2.getTrade().getPartner() == c1.getTrade())
            {
                c2.sendPacket(PacketCreator.getTradePartnerAdd(c1));
                c1.sendPacket(PacketCreator.getTradeStart(c1.getClient(), c1.getTrade(), 1));
                c1.getTrade().setFullTrade(true);
                c2.getTrade().setFullTrade(true);
            }
            else
            {
                c1.message("The other player has already closed the trade.");
            }
        }
        else
        {
            c1.message("This trade invitation already rescinded.");
            cancelTrade(c1, TradeResult.NO_RESPONSE);
        }
    }

    public static void declineTrade(IPlayer chr)
    {
        var trade = chr.getTrade();
        if (trade != null)
        {
            if (trade.getPartner() != null)
            {
                IPlayer other = trade.getPartner().getChr();
                if (InviteCoordinator.answerInvite(InviteType.TRADE, chr.getId(), other.getId(), false).result == InviteResultType.DENIED)
                {
                    other.message(chr.getName() + " has declined your trade request.");
                }

                other.getTrade().cancel((byte)TradeResult.PARTNER_CANCEL);
                other.setTrade(null);

            }
            trade.cancel((byte)TradeResult.NO_RESPONSE);
            chr.setTrade(null);
        }
    }

    public bool isFullTrade()
    {
        return fullTrade;
    }

    public void setFullTrade(bool fullTrade)
    {
        this.fullTrade = fullTrade;
    }

    private static void logTrade(Trade trade1, Trade trade2)
    {
        string name1 = trade1.getChr().getName();
        string name2 = trade2.getChr().getName();
        StringBuilder message = new StringBuilder();
        message.Append(string.Format("Committing trade between {0} and {1}", name1, name2));
        //Trade 1 to trade 2
        message.Append(string.Format("Trading {0} -> {1}: {2} mesos, items: {3}", name1, name2,
                trade1.getExchangeMesos(), getFormattedItemLogMessage(trade1.getItems())));

        //Trade 2 to trade 1
        message.Append(string.Format("Trading {0} -> {1}: {2} mesos, items: {3}", name2, name1,
                trade2.getExchangeMesos(), getFormattedItemLogMessage(trade2.getItems())));

        log.Information(message.ToString());
    }

    private static string getFormattedItemLogMessage(List<Item> items)
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        return string.Join(", ", items.Select(x => $"[{x.getQuantity()} {ii.getName(x.getItemId())} ({x.getItemId()})]"));
    }
}

public enum TradeResult
{
    NO_RESPONSE = 1,
    PARTNER_CANCEL = 2,
    SUCCESSFUL = 7,
    UNSUCCESSFUL = 8,
    UNSUCCESSFUL_UNIQUE_ITEM_LIMIT = 9,
    UNSUCCESSFUL_ANOTHER_MAP = 12,
    UNSUCCESSFUL_DAMAGED_FILES = 13
}