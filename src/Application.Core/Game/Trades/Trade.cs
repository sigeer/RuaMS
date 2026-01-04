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


using Application.Core.Game.Invites;
using client.inventory;
using client.inventory.manipulator;
using constants.game;
using tools;

namespace Application.Core.Game.Trades;


/**
 * @author Matze
 * @author Ronan - concurrency safety + check available slots + trade results
 */
public class Trade
{
    private ILogger log = LogFactory.GetLogger(LogType.Trade);

    public const int MaxItemCount = 9;
    public Trade? PartnerTrade { get; set; }
    public List<Item> ItemList { get; set; } = [];
    public int Meso { get; set; }
    private AtomicBoolean locked = new AtomicBoolean(false);
    private Player chr;
    private byte number;
    private bool fullTrade = false;

    public Trade(byte number, Player chr)
    {
        this.chr = chr;
        this.number = number;
    }

    private void lockTrade()
    {
        locked.Set(true);
        PartnerTrade.getChr().sendPacket(PacketCreator.getTradeConfirmation());
    }


    public void GainItemByComplete()
    {
        if (PartnerTrade == null)
            return;

        byte result;
        bool show = YamlConfig.config.server.USE_DEBUG;

        foreach (Item item in PartnerTrade.ItemList)
        {
            KarmaManipulator.toggleKarmaFlagToUntradeable(item);
            InventoryManipulator.addFromDrop(chr.Client, item, show);
        }

        if (PartnerTrade.Meso > 0)
        {
            int fee = TradeManager.GetFee(PartnerTrade.Meso);
            var actualGainMeso = PartnerTrade.Meso - fee;
            chr.gainMeso(actualGainMeso, show, true, show);
            if (fee > 0)
            {
                chr.dropMessage(1, "Transaction completed. You received " + chr.Client.CurrentCulture.Number(actualGainMeso) + " mesos due to trade fees.");
            }
            else
            {
                chr.dropMessage(1, "Transaction completed. You received " + chr.Client.CurrentCulture.Number(actualGainMeso) + " mesos.");
            }

            result = (byte)TradeResult.NO_RESPONSE;
        }
        else
        {
            result = (byte)TradeResult.SUCCESSFUL;
        }

        PartnerTrade.Meso = 0;
        PartnerTrade.ItemList.Clear();

        chr.sendPacket(PacketCreator.getTradeResult(number, result));
    }

    public void GainItemByCancel(byte result)
    {
        bool show = YamlConfig.config.server.USE_DEBUG;

        foreach (Item item in ItemList)
        {
            InventoryManipulator.addFromDrop(chr.Client, item, show);
        }
        if (Meso > 0)
        {
            chr.gainMeso(Meso, show, true, show);
        }
        Meso = 0;
        ItemList.Clear();

        chr.sendPacket(PacketCreator.getTradeResult(number, result));
    }

    public void CancelTrade(TradeResult result)
    {
        cancelHandshake((byte)result);
    }

    private bool isLocked()
    {
        return locked.Get();
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
            this.Meso += meso;
            chr.sendPacket(PacketCreator.getTradeMesoSet(0, this.Meso));

            if (PartnerTrade != null)
            {
                PartnerTrade.getChr().sendPacket(PacketCreator.getTradeMesoSet(1, this.Meso));
            }
        }
    }

    public bool addItem(Item item)
    {
        lock (ItemList)
        {
            if (ItemList.Count > MaxItemCount || ItemList.Any(x => x.getPosition() == item.getPosition()))
            {
                return false;
            }

            ItemList.Add(item);
        }

        return true;
    }

    public void chat(string message)
    {
        chr.sendPacket(PacketCreator.getTradeChat(chr, message, true));
        if (PartnerTrade != null)
        {
            PartnerTrade.getChr().sendPacket(PacketCreator.getTradeChat(chr, message, false));
        }
    }

    public void setPartner(Trade partner)
    {
        if (locked)
        {
            return;
        }
        this.PartnerTrade = partner;
    }

    public Player getChr()
    {
        return chr;
    }

    public bool fitsMeso()
    {
        if (PartnerTrade == null)
            return true;

        return chr.canHoldMeso(PartnerTrade.Meso - TradeManager.GetFee(PartnerTrade.Meso));
    }

    public bool fitsInInventory()
    {
        if (PartnerTrade == null)
            return true;

        var tradeItems = PartnerTrade.ItemList.Select(x => new ItemInventoryType(x, x.getInventoryType())).ToList();
        return Inventory.checkSpotsAndOwnership(chr, tradeItems);
    }

    public bool fitsUniquesInInventory()
    {
        if (PartnerTrade == null)
            return true;

        var exchangeItemids = PartnerTrade.ItemList.Select(x => x.getItemId()).ToList(); ;
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
                other = PartnerTrade!;
            }
            else
            {
                self = PartnerTrade!;
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

    public bool checkCompleteHandshake()
    {
        return checkTradeCompleteHandshake(true);
    }



    object cancelLock = new object();
    private void tradeCancelHandshake(bool updateSelf, byte result)
    {
        lock (cancelLock)
        {
            var (selfResult, partnerResult) = tradeResultsPair(result);

            var self = updateSelf ? this : PartnerTrade;
            CancelTradeInternal(self, selfResult, partnerResult);
        }
    }

    private void cancelHandshake(byte result)
    {
        tradeCancelHandshake(true, result);
    }


    private static void CancelTradeInternal(Trade? trade, byte selfResult, byte partnerResult)
    {
        if (trade == null)
            return;

        trade.GainItemByCancel(selfResult);
        if (trade.PartnerTrade != null)
        {
            trade.PartnerTrade.GainItemByCancel(partnerResult);
            trade.PartnerTrade.getChr().setTrade(null);

            InviteType.TRADE.AnswerInvite(trade.getChr().getId(), trade.PartnerTrade.getChr().getId(), false);
            InviteType.TRADE.AnswerInvite(trade.PartnerTrade.getChr().getId(), trade.getChr().getId(), false);
        }
        trade.getChr().setTrade(null);
    }

    public bool isFullTrade()
    {
        return fullTrade;
    }

    public void setFullTrade(bool fullTrade)
    {
        this.fullTrade = fullTrade;
    }

    private static (byte, byte) tradeResultsPair(byte result)
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

        return (selfResult, partnerResult);
    }
}
