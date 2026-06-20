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

    private async Task lockTrade()
    {
        locked.Set(true);
        await PartnerTrade.getChr().SendPacket(PacketCreator.getTradeConfirmation());
    }


    public async Task GainItemByComplete()
    {
        if (PartnerTrade == null)
            return;

        byte result;
        bool show = YamlConfig.config.server.USE_DEBUG;

        foreach (Item item in PartnerTrade.ItemList)
        {
            KarmaManipulator.toggleKarmaFlagToUntradeable(item);
            await InventoryManipulator.addFromDrop(chr.Client, item, show);
        }

        if (PartnerTrade.Meso > 0)
        {
            int fee = TradeManager.GetFee(PartnerTrade.Meso);
            var actualGainMeso = PartnerTrade.Meso - fee;
            await chr.GainMeso(actualGainMeso, GainItemShow.ShowInChat, true);
            if (fee > 0)
            {
                await chr.Popup("Transaction completed. You received " + chr.Client.CurrentCulture.Number(actualGainMeso) + " mesos due to trade fees.");
            }
            else
            {
                await chr.Popup("Transaction completed. You received " + chr.Client.CurrentCulture.Number(actualGainMeso) + " mesos.");
            }

            result = (byte)TradeResult.NO_RESPONSE;
        }
        else
        {
            result = (byte)TradeResult.SUCCESSFUL;
        }

        PartnerTrade.Meso = 0;
        PartnerTrade.ItemList.Clear();

        await chr.SendPacket(PacketCreator.getTradeResult(number, result));
    }

    public async Task GainItemByCancel(byte result)
    {
        bool show = YamlConfig.config.server.USE_DEBUG;

        foreach (Item item in ItemList)
        {
            await InventoryManipulator.addFromDrop(chr.Client, item, show);
        }
        if (Meso > 0)
        {
            await chr.GainMeso(Meso, GainItemShow.ShowInChat, true);
        }
        Meso = 0;
        ItemList.Clear();

        await chr.SendPacket(PacketCreator.getTradeResult(number, result));
    }

    public async Task CancelTrade(TradeResult result)
    {
        await cancelHandshake((byte)result);
    }

    private bool isLocked()
    {
        return locked.Get();
    }

    public async Task setMeso(int meso)
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
            await chr.GainMeso(-meso, enableActions: true);
            this.Meso += meso;
            await chr.SendPacket(PacketCreator.getTradeMesoSet(0, this.Meso));

            if (PartnerTrade != null)
            {
                await PartnerTrade.getChr().SendPacket(PacketCreator.getTradeMesoSet(1, this.Meso));
            }
        }
    }

    public bool addItem(Item item)
    {
        if (ItemList.Count > MaxItemCount || ItemList.Any(x => x.getPosition() == item.getPosition()))
        {
            return false;
        }

        ItemList.Add(item);

        return true;
    }

    public async Task chat(string message)
    {
        await chr.SendPacket(PacketCreator.getTradeChat(chr, message, true));
        if (PartnerTrade != null)
        {
            await PartnerTrade.getChr().SendPacket(PacketCreator.getTradeChat(chr, message, false));
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

    private async Task<bool> checkTradeCompleteHandshake(bool updateSelf)
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

        await self.lockTrade();
        return other.isLocked();
    }

    public async Task<bool> checkCompleteHandshake()
    {
        return await checkTradeCompleteHandshake(true);
    }



    private readonly SemaphoreSlim _cancelLock = new SemaphoreSlim(1, 1);
    private async Task tradeCancelHandshake(bool updateSelf, byte result)
    {
        await _cancelLock.WaitAsync();
        try
        {
            var (selfResult, partnerResult) = tradeResultsPair(result);
            var self = updateSelf ? this : PartnerTrade;
            await CancelTradeInternal(self, selfResult, partnerResult);
        }
        finally
        {
            _cancelLock.Release();
        }
    }

    private async Task cancelHandshake(byte result)
    {
        await tradeCancelHandshake(true, result);
    }


    private static async Task CancelTradeInternal(Trade? trade, byte selfResult, byte partnerResult)
    {
        if (trade == null)
            return;

        await trade.GainItemByCancel(selfResult);
        if (trade.PartnerTrade != null)
        {
            await trade.PartnerTrade.GainItemByCancel(partnerResult);
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
