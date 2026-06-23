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
using Application.Core.Channel.Services;
using Application.Core.Game.Items;
using Application.Core.Managers;
using Application.Resources.Messages;
using client.inventory;
using Microsoft.Extensions.Logging;
using server;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class CashOperationHandler : ChannelHandlerBase
{
    readonly ILogger<CashOperationHandler> _logger;
    readonly ItemService _itemService;
    readonly CashItemProvider _cashItemProvider;

    public CashOperationHandler(ILogger<CashOperationHandler> logger, ItemService itemService, CashItemProvider cashItemProvider)
    {
        _logger = logger;
        _itemService = itemService;
        _cashItemProvider = cashItemProvider;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        CashShop cs = chr.getCashShop();

        if (!cs.isOpened())
        {
            await c.SendPacket(PacketCreator.enableActions());
            return;
        }

        {
            await c.tryacquireClient();
            // thanks Thora for finding out an exploit within cash operations
            try
            {
                int action = p.readByte();
                if (action == 0x03 || action == 0x1E)
                {
                    p.readByte();
                    int useNX = p.readInt();
                    int snCS = p.readInt();
                    var cItem = _cashItemProvider.getItem(snCS);
                    if (cItem == null || !canBuy(chr, cItem, cs.getCash(useNX)))
                    {
                        _logger.LogError("Denied to sell cash item with SN {ItemSN}", snCS); // preventing NPE here thanks to MedicOP
                        await c.OnlinedCharacter.enableCSActions();
                        return;
                    }

                    if (action == 0x03)
                    {
                        // Item
                        if (ItemConstants.isCashStore(cItem!.getItemId()) && chr.getLevel() < 16)
                        {
                            await c.OnlinedCharacter.enableCSActions();
                            return;
                        }
                        else if (ItemConstants.isRateCoupon(cItem.getItemId()) && !YamlConfig.config.server.USE_SUPPLY_RATE_COUPONS)
                        {
                            await chr.dropMessage(1, "Rate coupons are currently unavailable to purchase.");
                            await c.OnlinedCharacter.enableCSActions();
                            return;
                        }
                        else if (ItemConstants.isMapleLife(cItem.getItemId()) && chr.getLevel() < 30)
                        {
                            await c.OnlinedCharacter.enableCSActions();
                            return;
                        }
                    }
                    await _itemService.BuyCashItem(c.OnlinedCharacter, useNX, cItem);
                }
                else if (action == 0x04)
                {
                    // TODO check for gender
                    int birthday = p.readInt();
                    var cashId = p.readInt();
                    var recipientName = p.readString();
                    var cItem = _cashItemProvider.getItem(cashId);
                    var message = p.readString();
                    if (cItem == null || !canBuy(chr, cItem, cs.getCash(CashShop.NX_PREPAID)) || string.IsNullOrEmpty(message) || message.Length > 73)
                    {
                        await c.OnlinedCharacter.enableCSActions();
                        return;
                    }
                    if (!c.CheckBirthday(birthday))
                    {
                        await c.SendPacket(PacketCreator.showCashShopMessage(0xC4));
                        return;
                    }
                    await _itemService.BuyCashItemForGift(c.OnlinedCharacter, CashType.NX_PREPAID, cItem, recipientName, message);
                }
                else if (action == 0x05)
                {
                    // Modify wish list
                    cs.clearWishList();
                    for (byte i = 0; i < 10; i++)
                    {
                        int sn = p.readInt();
                        var cItem = _cashItemProvider.getItem(sn);
                        if (cItem != null && cItem.isOnSale() && sn != 0)
                        {
                            cs.addToWishList(sn);
                        }
                    }
                    await c.SendPacket(PacketCreator.showWishList(chr, true));
                }
                else if (action == 0x06)
                {
                    // Increase Inventory Slots
                    p.skip(1);
                    int cash = p.readInt();
                    byte mode = p.readByte();
                    if (mode == 0)
                    {
                        sbyte type = p.ReadSByte();
                        if (cs.getCash(cash) < 4000)
                        {
                            await c.OnlinedCharacter.enableCSActions();
                            return;
                        }
                        int qty = 4;
                        if (!chr.canGainSlots(type, qty))
                        {
                            await c.OnlinedCharacter.enableCSActions();
                            return;
                        }
                        cs.gainCash(cash, -4000);
                        if (await chr.gainSlots(type, qty, false))
                        {
                            await c.SendPacket(PacketCreator.showBoughtInventorySlots(type, chr.getSlots(type)));
                            await c.SendPacket(PacketCreator.showCash(chr));
                        }
                        else
                        {
                            _logger.LogWarning("Could not add {Slot} slots of type {ItemType} for chr {CharacterName}", qty, type, CharacterManager.makeMapleReadable(chr.getName()));
                        }
                    }
                    else
                    {
                        var cItem = _cashItemProvider.getItem(p.readInt());
                        if (!canBuy(chr, cItem, cs.getCash(cash)))
                        {
                            await c.OnlinedCharacter.enableCSActions();
                            return;
                        }
                        int type = (cItem!.getItemId() - 9110000) / 1000;
                        int qty = 8;
                        if (!chr.canGainSlots(type, qty))
                        {
                            await c.OnlinedCharacter.enableCSActions();
                            return;
                        }

                        cs.Buy(cash, cItem);
                        if (await chr.gainSlots(type, qty, false))
                        {
                            await c.SendPacket(PacketCreator.showBoughtInventorySlots(type, chr.getSlots(type)));
                            await c.SendPacket(PacketCreator.showCash(chr));
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
                            await c.OnlinedCharacter.enableCSActions();
                            return;
                        }

                        int qty = 4;
                        if (chr.getStorage().TryGainSlots(qty))
                        {
                            cs.gainCash(cash, -4000);
                            _logger.LogDebug("Chr {CharacterName} bought {Slots} slots to their account storage.", c.OnlinedCharacter.getName(), qty);

                            await c.SendPacket(PacketCreator.showBoughtStorageSlots(chr.Storage.Slots));
                            await c.SendPacket(PacketCreator.showCash(chr));
                        }
                        else
                        {
                            _logger.LogWarning("Could not add {Slot} slots to {CharacterName}'s account.", qty, CharacterManager.makeMapleReadable(chr.getName()));
                            await c.OnlinedCharacter.enableCSActions();
                            return;
                        }
                    }
                    else
                    {
                        var cItem = _cashItemProvider.getItem(p.readInt());

                        if (!canBuy(chr, cItem, cs.getCash(cash)))
                        {
                            await c.OnlinedCharacter.enableCSActions();
                            return;
                        }
                        int qty = 8;
                        if (chr.getStorage().TryGainSlots(qty))
                        {
                            cs.Buy(cash, cItem);
                            // thanks ABaldParrot & Thora for detecting storage issues here
                            _logger.LogDebug("Chr {CharacterName} bought {Slot} slots to their account storage", c.OnlinedCharacter.getName(), qty);

                            await c.SendPacket(PacketCreator.showBoughtStorageSlots(chr.Storage.Slots));
                            await c.SendPacket(PacketCreator.showCash(chr));
                        }
                        else
                        {
                            _logger.LogWarning("Could not add {Slot} slots to {CharacterName}'s account", qty, CharacterManager.makeMapleReadable(chr.getName()));
                            await c.OnlinedCharacter.enableCSActions();
                            return;
                        }
                    }
                }
                else if (action == 0x08)
                {
                    // Increase Character Slots
                    p.skip(1);
                    int cash = p.readInt();
                    var cItem = _cashItemProvider.getItem(p.readInt());

                    if (!canBuy(chr, cItem, cs.getCash(cash)))
                    {
                        await c.OnlinedCharacter.enableCSActions();
                        return;
                    }

                    if (c.GainCharacterSlot())
                    {
                        cs.Buy(cash, cItem);
                        await c.SendPacket(PacketCreator.showBoughtCharacterSlot(c.AccountEntity!.Characterslots));
                        await c.SendPacket(PacketCreator.showCash(chr));
                    }
                    else
                    {
                        await chr.Popup("You have already used up all 12 extra character slots.");
                        await c.OnlinedCharacter.enableCSActions();
                        return;
                    }
                }
                else if (action == 0x0D)
                {
                    // Take from Cash Inventory
                    var item = cs.findByCashId(p.readLong());
                    if (item == null)
                    {
                        await c.OnlinedCharacter.enableCSActions();
                        return;
                    }

                    var addR = await chr.GetInventory(item.getInventoryType()).AddItem(item);
                    if (addR != null)
                    {
                        cs.removeFromInventory(item);
                        await c.SendPacket(PacketCreator.takeFromCashInventory(item, addR.CurrentPosition));

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
                        await c.Disconnect(false, false);
                        return;
                    }

                    var mi = chr.getInventory(InventoryTypeUtils.getByType(invType));
                    var item = mi.findByCashId(cashId);
                    if (item == null)
                    {
                        await c.OnlinedCharacter.enableCSActions();
                        return;
                    }
                    else if (item is Pet pet && pet.Summoned)
                    {
                        await chr.Popup(nameof(ClientMessage.Cash_PutPetIntoCashInventory));
                        await c.OnlinedCharacter.enableCSActions();
                        return;
                    }
                    else if (ItemId.isWeddingRing(item.getItemId()) || ItemId.isWeddingToken(item.getItemId()))
                    {
                        await chr.Popup(nameof(ClientMessage.Cash_PutRelationshipItemIntoCashInventory));
                        await c.OnlinedCharacter.enableCSActions();
                        return;
                    }
                    cs.addToInventory(item);
                    await mi.removeSlot(item.getPosition());
                    await c.SendPacket(PacketCreator.putIntoCashInventory(item, c.AccountEntity!.Id));
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
                        var itemRing = _cashItemProvider.GetItemTrust(SN);
                        var partner = c.CurrentServer.getPlayerStorage().getCharacterByName(recipientName);
                        await _itemService.BuyCashItemForGift(c.OnlinedCharacter, toCharge, itemRing, recipientName, text, true);
                    }
                    else
                    {
                        await c.SendPacket(PacketCreator.showCashShopMessage(0xC4));
                    }
                }
                else if (action == 0x20)
                {
                    // 金币购买用于任务的物品
                    int serialNumber = p.readInt();  // thanks GabrielSin for detecting a potential exploit with 1 meso cash items.
                    if (serialNumber / 10000000 != 8)
                    {
                        await c.SendPacket(PacketCreator.showCashShopMessage(0xC0));
                        return;
                    }

                    var item = _cashItemProvider.getItem(serialNumber);
                    if (item == null || !item.isOnSale())
                    {
                        await c.SendPacket(PacketCreator.showCashShopMessage(0xC0));
                        return;
                    }

                    int itemId = item.getItemId();
                    int itemPrice = item.getPrice();
                    if (itemPrice <= 0)
                    {
                        await c.SendPacket(PacketCreator.showCashShopMessage(0xC0));
                        return;
                    }

                    if (chr.getMeso() >= itemPrice)
                    {
                        if (chr.canHold(itemId))
                        {
                            await chr.GainMeso(-itemPrice);
                            await chr.GainItem(itemId, 1);
                            await c.SendPacket(PacketCreator.showBoughtQuestItem(itemId));
                        }
                    }
                    await c.SendPacket(PacketCreator.showCash(c.OnlinedCharacter));
                }
                else if (action == 0x23)
                {
                    //Friendship :3
                    int birthday = p.readInt();
                    if (c.CheckBirthday(birthday))
                    {
                        int payment = p.readInt();
                        int snID = p.readInt();
                        var itemRing = _cashItemProvider.GetItemTrust(snID);
                        string sentTo = p.readString();
                        string text = p.readString();
                        await _itemService.BuyCashItemForGift(c.OnlinedCharacter, payment, itemRing, sentTo, text, true);
                    }
                    else
                    {
                        await c.SendPacket(PacketCreator.showCashShopMessage(0xC4));
                    }
                }
                else if (action == 0x2E)
                {
                    // TODO: 暂不支持，后续修复
                    //name change
                    var cItem = _cashItemProvider.getItem(p.readInt());
                    if (cItem == null || !canBuy(chr, cItem, cs.getCash(CashShop.NX_PREPAID)))
                    {
                        await c.SendPacket(PacketCreator.showCashShopMessage(0));
                        await c.OnlinedCharacter.enableCSActions();
                        return;
                    }
                    if (cItem.getSN() == 50600000 && YamlConfig.config.server.ALLOW_CASHSHOP_NAME_CHANGE)
                    {
                        p.readString(); //old name
                        string newName = p.readString();
                        //else if (c.AccountEntity?.Tempban != null && (c.AccountEntity.Tempban!.Value.AddDays(30)) > DateTimeOffset.UtcNow)
                        //{
                        //    await c.SendPacket(PacketCreator.showCashShopMessage(0));
                        //    c.OnlinedCharacter.enableCSActions();
                        //    return;
                        //}

                        if (_itemService.RegisterNameChange(c.OnlinedCharacter, newName))
                        {
                            //success
                            cs.Buy(CashType.NX_PREPAID, cItem);

                            Item item = _itemService.CashItem2Item(cItem);
                            await c.SendPacket(PacketCreator.showNameChangeSuccess(item, c.AccountEntity!.Id));
                            // cs.addToInventory(item);

                        }
                        else
                        {
                            await c.OnlinedCharacter.enableCSActions();
                            return;
                        }
                    }
                    await c.OnlinedCharacter.enableCSActions();
                }
                else if (action == 0x31)
                {
                    //world transfer 
                    // 移除了大区的概念，转区没有意义
                    throw new BusinessNotsupportException();
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
    }

    private bool canBuy(Player chr, CashItem? item, int cash)
    {
        if (item != null && item.isOnSale() && item.getPrice() <= cash)
        {
            _logger.LogDebug("Chr {CharacterName} bought cash item {ItemName} (SN {ItemSN}) for {ItemPrice}",
                chr,
                ClientCulture.SystemCulture.GetItemName(item.getItemId()),
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
