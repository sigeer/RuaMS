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


using Application.Core.Game.Items;
using Application.Core.Game.Players;
using Application.Core.Managers;
using Application.Shared.Items;
using Application.Utility.Configs;
using Application.Utility.Exceptions;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.Extensions.Logging;
using server;
using tools;
using static server.CashShop;

namespace Application.Core.Channel.Net.Handlers;

public class CashOperationHandler : ChannelHandlerBase
{
    readonly ILogger<CashOperationHandler> _logger;

    public CashOperationHandler(ILogger<CashOperationHandler> logger)
    {
        _logger = logger;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        CashShop cs = chr.getCashShop();

        if (!cs.isOpened())
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        if (c.tryacquireClient())
        {     // thanks Thora for finding out an exploit within cash operations
            try
            {
                int action = p.readByte();
                if (action == 0x03 || action == 0x1E)
                {
                    p.readByte();
                    int useNX = p.readInt();
                    int snCS = p.readInt();
                    var cItem = CashItemFactory.getItem(snCS);
                    if (!canBuy(chr, cItem, cs.getCash(useNX)))
                    {
                        _logger.LogError("Denied to sell cash item with SN {ItemSN}", snCS); // preventing NPE here thanks to MedicOP
                        c.enableCSActions();
                        return;
                    }

                    if (action == 0x03)
                    { // Item
                        if (ItemConstants.isCashStore(cItem!.getItemId()) && chr.getLevel() < 16)
                        {
                            c.enableCSActions();
                            return;
                        }
                        else if (ItemConstants.isRateCoupon(cItem.getItemId()) && !YamlConfig.config.server.USE_SUPPLY_RATE_COUPONS)
                        {
                            chr.dropMessage(1, "Rate coupons are currently unavailable to purchase.");
                            c.enableCSActions();
                            return;
                        }
                        else if (ItemConstants.isMapleLife(cItem.getItemId()) && chr.getLevel() < 30)
                        {
                            c.enableCSActions();
                            return;
                        }

                        Item item = chr.getChannelServer().Service.CashItem2Item(cItem);
                        cs.gainCash(useNX, cItem, chr.getWorld());  // thanks Rohenn for noticing cash operations after item acquisition
                        cs.addToInventory(item);
                        c.sendPacket(PacketCreator.showBoughtCashItem(item, c.AccountEntity!.Id));
                    }
                    else
                    {
                        // Package
                        cs.gainCash(useNX, cItem, chr.getWorld());

                        List<Item> cashPackage = CashItemFactory.getPackage(cItem.getItemId(), chr.getChannelServer().Service);
                        foreach (Item item in cashPackage)
                        {
                            cs.addToInventory(item);
                        }
                        c.sendPacket(PacketCreator.showBoughtCashPackage(cashPackage, c.AccountEntity!.Id));
                    }
                    c.sendPacket(PacketCreator.showCash(chr));
                }
                else if (action == 0x04)
                {
                    //TODO check for gender
                    int birthday = p.readInt();
                    var cItem = CashItemFactory.getItem(p.readInt());
                    var recipient = CharacterManager.GetCharacterFromDatabase(p.readString());
                    var message = p.readString();
                    if (canBuy(chr, cItem, cs.getCash(CashShop.NX_PREPAID)) || string.IsNullOrEmpty(message) || message.Length > 73)
                    {
                        c.enableCSActions();
                        return;
                    }
                    if (!c.CheckBirthday(birthday))
                    {
                        c.sendPacket(PacketCreator.showCashShopMessage(0xC4));
                        return;
                    }
                    else if (recipient == null)
                    {
                        c.sendPacket(PacketCreator.showCashShopMessage(0xA9));
                        return;
                    }
                    else if (recipient.AccountId == c.AccountEntity!.Id)
                    {
                        c.sendPacket(PacketCreator.showCashShopMessage(0xA8));
                        return;
                    }
                    cs.gainCash(4, cItem, chr.getWorld());
                    cs.gift(recipient.CharacterId, chr.getName(), message, cItem.getSN());
                    c.sendPacket(PacketCreator.showGiftSucceed(recipient.CharacterName, cItem));
                    c.sendPacket(PacketCreator.showCash(chr));

                    string noteMessage = chr.getName() + " has sent you a gift! Go check out the Cash Shop.";
                    c.CurrentServerContainer.Transport.SendNormalNoteMessage(chr.Name, recipient.CharacterName, noteMessage);
                }
                else if (action == 0x05)
                {
                    // Modify wish list
                    cs.clearWishList();
                    for (byte i = 0; i < 10; i++)
                    {
                        int sn = p.readInt();
                        var cItem = CashItemFactory.getItem(sn);
                        if (cItem != null && cItem.isOnSale() && sn != 0)
                        {
                            cs.addToWishList(sn);
                        }
                    }
                    c.sendPacket(PacketCreator.showWishList(chr, true));
                }
                else if (action == 0x06)
                { // Increase Inventory Slots
                    p.skip(1);
                    int cash = p.readInt();
                    byte mode = p.readByte();
                    if (mode == 0)
                    {
                        sbyte type = p.ReadSByte();
                        if (cs.getCash(cash) < 4000)
                        {
                            c.enableCSActions();
                            return;
                        }
                        int qty = 4;
                        if (!chr.canGainSlots(type, qty))
                        {
                            c.enableCSActions();
                            return;
                        }
                        cs.gainCash(cash, -4000);
                        if (chr.gainSlots(type, qty, false))
                        {
                            c.sendPacket(PacketCreator.showBoughtInventorySlots(type, chr.getSlots(type)));
                            c.sendPacket(PacketCreator.showCash(chr));
                        }
                        else
                        {
                            _logger.LogWarning("Could not add {Slot} slots of type {ItemType} for chr {CharacterName}", qty, type, CharacterManager.makeMapleReadable(chr.getName()));
                        }
                    }
                    else
                    {
                        var cItem = CashItemFactory.getItem(p.readInt());
                        if (!canBuy(chr, cItem, cs.getCash(cash)))
                        {
                            c.enableCSActions();
                            return;
                        }
                        int type = (cItem!.getItemId() - 9110000) / 1000;
                        int qty = 8;
                        if (!chr.canGainSlots(type, qty))
                        {
                            c.enableCSActions();
                            return;
                        }
                        cs.gainCash(cash, cItem, chr.getWorld());
                        if (chr.gainSlots(type, qty, false))
                        {
                            c.sendPacket(PacketCreator.showBoughtInventorySlots(type, chr.getSlots(type)));
                            c.sendPacket(PacketCreator.showCash(chr));
                        }
                        else
                        {
                            _logger.LogWarning("Could not add {Slot} slots of type {ItemType} for chr {CharacterName}", qty, type, CharacterManager.makeMapleReadable(chr.getName()));
                        }
                    }
                }
                else if (action == 0x07)
                {
                    // Increase Storage Slots
                    p.skip(1);
                    int cash = p.readInt();
                    byte mode = p.readByte();
                    if (mode == 0)
                    {
                        if (cs.getCash(cash) < 4000)
                        {
                            c.enableCSActions();
                            return;
                        }
                        int qty = 4;
                        if (!chr.getStorage().canGainSlots(qty))
                        {
                            c.enableCSActions();
                            return;
                        }
                        cs.gainCash(cash, -4000);
                        if (chr.getStorage().gainSlots(qty))
                        {
                            _logger.LogDebug("Chr {CharacterName} bought {Slots} slots to their account storage.", c.OnlinedCharacter.getName(), qty);

                            c.sendPacket(PacketCreator.showBoughtStorageSlots(chr.getStorage().getSlots()));
                            c.sendPacket(PacketCreator.showCash(chr));
                        }
                        else
                        {
                            _logger.LogWarning("Could not add {Slot} slots to {CharacterName}'s account.", qty, CharacterManager.makeMapleReadable(chr.getName()));
                        }
                    }
                    else
                    {
                        var cItem = CashItemFactory.getItem(p.readInt());

                        if (!canBuy(chr, cItem, cs.getCash(cash)))
                        {
                            c.enableCSActions();
                            return;
                        }
                        int qty = 8;
                        if (!chr.getStorage().canGainSlots(qty))
                        {
                            c.enableCSActions();
                            return;
                        }
                        cs.gainCash(cash, cItem, chr.getWorld());
                        if (chr.getStorage().gainSlots(qty))
                        {
                            // thanks ABaldParrot & Thora for detecting storage issues here
                            _logger.LogDebug("Chr {CharacterName} bought {Slot} slots to their account storage", c.OnlinedCharacter.getName(), qty);

                            c.sendPacket(PacketCreator.showBoughtStorageSlots(chr.getStorage().getSlots()));
                            c.sendPacket(PacketCreator.showCash(chr));
                        }
                        else
                        {
                            _logger.LogWarning("Could not add {Slot} slots to {CharacterName}'s account", qty, CharacterManager.makeMapleReadable(chr.getName()));
                        }
                    }
                }
                else if (action == 0x08)
                {
                    // Increase Character Slots
                    p.skip(1);
                    int cash = p.readInt();
                    var cItem = CashItemFactory.getItem(p.readInt());

                    if (!canBuy(chr, cItem, cs.getCash(cash)))
                    {
                        c.enableCSActions();
                        return;
                    }
                    if (!c.CanGainCharacterSlot())
                    {
                        chr.dropMessage(1, "You have already used up all 12 extra character slots.");
                        c.enableCSActions();
                        return;
                    }
                    cs.gainCash(cash, cItem, chr.getWorld());
                    if (c.GainCharacterSlot())
                    {
                        c.sendPacket(PacketCreator.showBoughtCharacterSlot(c.AccountEntity!.Characterslots));
                        c.sendPacket(PacketCreator.showCash(chr));
                    }
                    else
                    {
                        _logger.LogWarning("Could not add a chr slot to {CharacterName}'s account", CharacterManager.makeMapleReadable(chr.getName()));
                        c.enableCSActions();
                        return;
                    }
                }
                else if (action == 0x0D)
                {
                    // Take from Cash Inventory
                    var item = cs.findByCashId(p.readLong());
                    if (item == null)
                    {
                        c.enableCSActions();
                        return;
                    }
                    if (chr.getInventory(item.getInventoryType()).addItem(item) != -1)
                    {
                        cs.removeFromInventory(item);
                        c.sendPacket(PacketCreator.takeFromCashInventory(item));

                        if (item is Equip equip)
                        {
                            if (equip.Ring != null)
                            {
                                chr.addPlayerRing(equip.Ring);
                            }
                        }
                    }
                }
                else if (action == 0x0E)
                {
                    // Put into Cash Inventory
                    var cashId = p.readLong();

                    sbyte invType = p.ReadSByte();
                    if (invType < 1 || invType > 5)
                    {
                        c.Disconnect(false, false);
                        return;
                    }

                    Inventory mi = chr.getInventory(InventoryTypeUtils.getByType(invType));
                    var item = mi.findByCashId(cashId);
                    if (item == null)
                    {
                        c.enableCSActions();
                        return;
                    }
                    else if (item is Pet pet && c.OnlinedCharacter.getPetIndex(pet.PetId) > -1)
                    {
                        chr.getClient().sendPacket(PacketCreator.serverNotice(1, "You cannot put the pet you currently equip into the Cash Shop inventory."));
                        c.enableCSActions();
                        return;
                    }
                    else if (ItemId.isWeddingRing(item.getItemId()) || ItemId.isWeddingToken(item.getItemId()))
                    {
                        chr.getClient().sendPacket(PacketCreator.serverNotice(1, "You cannot put relationship items into the Cash Shop inventory."));
                        c.enableCSActions();
                        return;
                    }
                    cs.addToInventory(item);
                    mi.removeSlot(item.getPosition());
                    c.sendPacket(PacketCreator.putIntoCashInventory(item, c.AccountEntity!.Id));
                }
                else if (action == 0x1D)
                {
                    //crush ring (action 28)
                    int birthday = p.readInt();
                    if (c.CheckBirthday(birthday))
                    {
                        int toCharge = p.readInt();
                        int SN = p.readInt();
                        string recipientName = p.readString();
                        string text = p.readString();
                        var itemRing = CashItemFactory.GetItemTrust(SN);
                        var partner = c.CurrentServer.getPlayerStorage().getCharacterByName(recipientName);
                        if (partner == null)
                        {
                            chr.sendPacket(PacketCreator.serverNotice(1, "The partner you specified cannot be found.\r\nPlease make sure your partner is online and in the same channel."));
                        }
                        else
                        {

                            /*  if (partner.getGender() == chr.getGender()) {
                                  chr.dropMessage(5, "You and your partner are the same gender, please buy a friendship ring.");
                                  c.enableCSActions();
                                  return;
                              }*/ //Gotta let them faggots marry too, hence why this is commented out <3 

                            if (chr.getChannelServer().Service.CashItem2Item(itemRing) is Equip eqp)
                            {
                                var rings = RingManager.CreateRing(itemRing.getItemId(), chr, partner);
                                if (rings == null)
                                {
                                    c.sendPacket(PacketCreator.showCashShopMessage(0xBE));
                                    return;
                                }
                                eqp.Ring = rings.MyRing;
                                cs.addToInventory(eqp);
                                c.sendPacket(PacketCreator.showBoughtCashItem(eqp, c.AccountEntity!.Id));
                                cs.gainCash(toCharge, itemRing, chr.getWorld());
                                cs.gift(partner.getId(), chr.getName(), text, eqp.getSN(), rings.PartnerRing.getRingId());
                                chr.addCrushRing(rings.MyRing);
                                c.CurrentServerContainer.Transport.SendFameNoteMessage(chr.Name, partner.Name, text);
                            }
                        }
                    }
                    else
                    {
                        c.sendPacket(PacketCreator.showCashShopMessage(0xC4));
                    }

                    c.sendPacket(PacketCreator.showCash(c.OnlinedCharacter));
                }
                else if (action == 0x20)
                {
                    int serialNumber = p.readInt();  // thanks GabrielSin for detecting a potential exploit with 1 meso cash items.
                    if (serialNumber / 10000000 != 8)
                    {
                        c.sendPacket(PacketCreator.showCashShopMessage(0xC0));
                        return;
                    }

                    var item = CashItemFactory.getItem(serialNumber);
                    if (item == null || !item.isOnSale())
                    {
                        c.sendPacket(PacketCreator.showCashShopMessage(0xC0));
                        return;
                    }

                    int itemId = item.getItemId();
                    int itemPrice = item.getPrice();
                    if (itemPrice <= 0)
                    {
                        c.sendPacket(PacketCreator.showCashShopMessage(0xC0));
                        return;
                    }

                    if (chr.getMeso() >= itemPrice)
                    {
                        if (chr.canHold(itemId))
                        {
                            chr.gainMeso(-itemPrice, false);
                            InventoryManipulator.addById(c, itemId, 1, "");
                            c.sendPacket(PacketCreator.showBoughtQuestItem(itemId));
                        }
                    }
                    c.sendPacket(PacketCreator.showCash(c.OnlinedCharacter));
                }
                else if (action == 0x23)
                { //Friendship :3
                    int birthday = p.readInt();
                    if (c.CheckBirthday(birthday))
                    {
                        int payment = p.readInt();
                        int snID = p.readInt();
                        var itemRing = CashItemFactory.GetItemTrust(snID);
                        string sentTo = p.readString();
                        string text = p.readString();
                        var partner = c.CurrentServer.getPlayerStorage().getCharacterByName(sentTo);
                        if (partner == null)
                        {
                            c.sendPacket(PacketCreator.showCashShopMessage(0xBE));
                        }
                        else
                        {
                            // Need to check to make sure its actually an equip and the right SN...
                            if (chr.getChannelServer().Service.CashItem2Item(itemRing) is Equip eqp)
                            {
                                var rings = RingManager.CreateRing(itemRing.getItemId(), chr, partner);
                                if (rings == null)
                                {
                                    c.sendPacket(PacketCreator.showCashShopMessage(0xBE));
                                    return;
                                }
                                eqp.Ring = rings.MyRing;
                                cs.addToInventory(eqp);
                                c.sendPacket(PacketCreator.showBoughtCashRing(eqp, partner.getName(), c.AccountEntity!.Id));
                                cs.gainCash(payment, -itemRing.getPrice());
                                cs.gift(partner.getId(), chr.getName(), text, eqp.getSN(), rings.PartnerRing.getRingId());
                                chr.addFriendshipRing(rings.MyRing);
                                c.CurrentServerContainer.Transport.SendFameNoteMessage(chr.Name, partner.Name, text);
                            }
                        }
                    }
                    else
                    {
                        c.sendPacket(PacketCreator.showCashShopMessage(0xC4));
                    }

                    c.sendPacket(PacketCreator.showCash(c.OnlinedCharacter));
                }
                else if (action == 0x2E)
                { //name change
                    var cItem = CashItemFactory.getItem(p.readInt());
                    if (cItem == null || !canBuy(chr, cItem, cs.getCash(CashShop.NX_PREPAID)))
                    {
                        c.sendPacket(PacketCreator.showCashShopMessage(0));
                        c.enableCSActions();
                        return;
                    }
                    if (cItem.getSN() == 50600000 && YamlConfig.config.server.ALLOW_CASHSHOP_NAME_CHANGE)
                    {
                        p.readString(); //old name
                        string newName = p.readString();
                        if (!c.CurrentServerContainer.CheckCharacterName(newName) || chr.getLevel() < 10)
                        { //(longest ban duration isn't tracked currently)
                            c.sendPacket(PacketCreator.showCashShopMessage(0));
                            c.enableCSActions();
                            return;
                        }
                        else if (c.AccountEntity?.Tempban != null && (c.AccountEntity.Tempban!.Value.AddDays(30)) > DateTimeOffset.UtcNow)
                        {
                            c.sendPacket(PacketCreator.showCashShopMessage(0));
                            c.enableCSActions();
                            return;
                        }
                        if (chr.registerNameChange(newName))
                        { //success
                            Item item = chr.getChannelServer().Service.CashItem2Item(cItem);
                            c.sendPacket(PacketCreator.showNameChangeSuccess(item, c.AccountEntity!.Id));
                            cs.gainCash(4, cItem, chr.getWorld());
                            cs.addToInventory(item);
                        }
                        else
                        {
                            c.sendPacket(PacketCreator.showCashShopMessage(0));
                        }
                    }
                    c.enableCSActions();
                }
                else if (action == 0x31)
                {
                    //world transfer
                    var cItem = CashItemFactory.getItem(p.readInt());
                    if (cItem == null || !canBuy(chr, cItem, cs.getCash(CashShop.NX_PREPAID)))
                    {
                        c.sendPacket(PacketCreator.showCashShopMessage(0));
                        c.enableCSActions();
                        return;
                    }
                    if (cItem.getSN() == 50600001)
                    {
                        throw new BusinessNotsupportException("World Transfer");
                    }
                    c.enableCSActions();
                }
                else
                {
                    _logger.LogWarning("Unhandled action: {Action}, packet: {Packet}", action, p);
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
        else
        {
            c.sendPacket(PacketCreator.enableActions());
        }
    }

    private bool canBuy(IPlayer chr, CashItem? item, int cash)
    {
        if (item != null && item.isOnSale() && item.getPrice() <= cash)
        {
            _logger.LogDebug("Chr {CharacterName} bought cash item {ItemName} (SN {ItemSN}) for {ItemPrice}",
                chr,
                ItemInformationProvider.getInstance().getName(item.getItemId()),
                item.getSN(),
                item.getPrice());
            return true;
        }
        else
        {
            return false;
        }
    }
}
