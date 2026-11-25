/*
This file is part of the OdinMS Maple Story NewServer
Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
Matthias Butz <matze@odinms.de>
Jan Christian Meyer <vimes@odinms.de>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License version 3
as published by the Free Software Foundation. You may not use, modify
or distribute this program under any other version of the
GNU Affero General Public License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Application.Core.Channel.ServerData;
using Application.Core.Managers;
using Application.Core.Game.Trades;
using Application.Core.Server;
using Application.Shared.Constants;
using Application.Shared.Constants.Item;
using client.inventory;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/// <summary>
/// 商城内操作。所有动作都由 <see cref="CashShopManager"/> 处理：
/// 需要 WZ 模板生成的物品（购买、赠送）也在那里按模板生成，
/// 记账、落仓、扣费则统一发生在中心服。
/// </summary>
public class CashOperationHandler : ChannelHandlerBase
{
    readonly ILogger<CashOperationHandler> _logger;
    readonly CashShopManager _cashShopManager;

    public CashOperationHandler(ILogger<CashOperationHandler> logger, CashShopManager cashShopManager)
    {
        _logger = logger;
        _cashShopManager = cashShopManager;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        if (!c.OnlinedCharacter.IsInExtraServer())
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
                    await HandleBuy(p, c, action);
                }
                else if (action == 0x04)
                {
                    await HandleGift(p, c);
                }
                else if (action == 0x05)
                {
                    // Modify wish list
                    var wishList = ParseWishList(p);
                    await _cashShopManager.UpdateWishList(c, wishList);
                }
                else if (action == 0x06)
                {
                    // Increase Inventory Slots
                    // 余额够不够、格数有没有到上限、扣多少钱，全部由中心服判断；
                    // 频道只把「买哪一页、加几格、多少钱」送过去（价格和页数来自 WZ 模板，中心服没有）
                    p.skip(1);
                    int cash = p.readInt();
                    byte mode = p.readByte();

                    int price, inventoryType, slots;
                    if (mode == 0)
                    {
                        inventoryType = p.ReadSByte();
                        price = 4000;
                        slots = 4;
                    }
                    else
                    {
                        var cItem = _cashShopManager.GetItem(p.readInt());
                        if (cItem == null || !cItem.OnSale)
                        {
                            await _cashShopManager.EnableActions(c);
                            return;
                        }
                        inventoryType = (cItem.ItemID - 9110000) / 1000;
                        price = cItem.Price;
                        slots = 8;
                    }

                    await _cashShopManager.GainInventorySlots(c, cash, price, inventoryType, slots);
                }
                else if (action == 0x07)
                {
                    // Increase Storage Slots
                    // 同样只送参数：扣费与上限判断在中心服
                    p.skip(1);
                    int cash = p.readInt();
                    byte mode = p.readByte();

                    int price, slots;
                    if (mode == 0)
                    {
                        price = 4000;
                        slots = 4;
                    }
                    else
                    {
                        var cItem = _cashShopManager.GetItem(p.readInt());
                        if (cItem == null || !cItem.OnSale)
                        {
                            await _cashShopManager.EnableActions(c);
                            return;
                        }
                        price = cItem.Price;
                        slots = 8;
                    }

                    await _cashShopManager.GainStorageSlots(c, cash, price, slots);
                }
                else if (action == 0x08)
                {
                    // Increase Character Slots
                    p.skip(1);
                    int cash = p.readInt();
                    var cItem = _cashShopManager.GetItem(p.readInt());
                    if (cItem == null || !cItem.OnSale)
                    {
                        await _cashShopManager.EnableActions(c);
                        return;
                    }

                    await _cashShopManager.GainCharacterSlots(c, cash, cItem.Price);
                }
                else if (action == 0x0D)
                {
                    // Take from Cash Inventory
                    await _cashShopManager.TakeFromCashInventory(c, p.readLong());
                }
                else if (action == 0x0E)
                {
                    // Put into Cash Inventory
                    var cashId = p.readLong();
                    await _cashShopManager.PutIntoCashInventory(c, cashId, p.ReadSByte());
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
                        var itemRing = _cashShopManager.GetItem(SN);
                        if (itemRing == null)
                        {
                            await _cashShopManager.EnableActions(c);
                            return;
                        }
                        await _cashShopManager.BuyCashItemForGift(c, toCharge, itemRing, recipientName, text, true);
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

                    var item = _cashShopManager.GetItem(serialNumber);
                    if (item == null || !item.OnSale)
                    {
                        await c.SendPacket(PacketCreator.showCashShopMessage(0xC0));
                        return;
                    }

                    int itemId = item.ItemID;
                    int itemPrice = item.Price;
                    if (itemPrice <= 0)
                    {
                        await c.SendPacket(PacketCreator.showCashShopMessage(0xC0));
                        return;
                    }

                    // 金币够不够、背包放不放得下都由中心服判断
                    await _cashShopManager.BuyCashItemForQuest(c, item);
                }
                else if (action == 0x23)
                {
                    //Friendship :3
                    int birthday = p.readInt();
                    if (c.CheckBirthday(birthday))
                    {
                        int payment = p.readInt();
                        int snID = p.readInt();
                        var itemRing = _cashShopManager.GetItem(snID);
                        string sentTo = p.readString();
                        string text = p.readString();
                        if (itemRing == null)
                        {
                            await _cashShopManager.EnableActions(c);
                            return;
                        }
                        await _cashShopManager.BuyCashItemForGift(c, payment, itemRing, sentTo, text, true);
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
                    var cItem = _cashShopManager.GetItem(p.readInt());
                    if (cItem == null || !_cashShopManager.CanBuy(c, cItem, GetCash(c, CashShopServer.NX_PREPAID)))
                    {
                        await c.SendPacket(PacketCreator.showCashShopMessage(0));
                        await _cashShopManager.EnableActions(c);
                        return;
                    }
                    if (cItem.CashItemSN == 50600000 && YamlConfig.config.server.ALLOW_CASHSHOP_NAME_CHANGE)
                    {
                        p.readString(); //old name
                        string newName = p.readString();

                        if (_cashShopManager.RegisterNameChange(c, newName))
                        {
                            //success
                            // 改名券是立即消耗的，不进现金仓；扣费仍然要在中心服做
                            await _cashShopManager.BuyConsumedCashItem(c, CashType.NX_PREPAID, cItem);

                            Item item = _cashShopManager.CashItem2Item(cItem);
                            await c.SendPacket(PacketCreator.showNameChangeSuccess(item, c.AccountEntity!.Id));
                        }
                        else
                        {
                            await _cashShopManager.EnableActions(c);
                            return;
                        }
                    }
                    await _cashShopManager.EnableActions(c);
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

    /// <summary>
    /// op 0x03 / 0x1E：购买（0x1E 是批量购买，字段与 0x03 相同）
    /// </summary>
    async Task HandleBuy(InPacket p, IChannelClient c, int action)
    {
        var chr = c.OnlinedCharacter;

        p.readByte();
        int useNX = p.readInt();
        int snCS = p.readInt();
        var cItem = _cashShopManager.GetItem(snCS);
        if (cItem == null || !_cashShopManager.CanBuy(c, cItem, GetCash(c, useNX)))
        {
            _logger.LogError("Denied to sell cash item with SN {ItemSN}", snCS); // preventing NPE here thanks to MedicOP
            await _cashShopManager.EnableActions(c);
            return;
        }

        if (action == 0x03)
        {
            // Item
            if (ItemConstants.isCashStore(cItem!.ItemID) && chr.getLevel() < 16)
            {
                await _cashShopManager.EnableActions(c);
                return;
            }
            else if (ItemConstants.isRateCoupon(cItem.ItemID) && !YamlConfig.config.server.USE_SUPPLY_RATE_COUPONS)
            {
                await chr.dropMessage(1, "Rate coupons are currently unavailable to purchase.");
                await _cashShopManager.EnableActions(c);
                return;
            }
            else if (ItemConstants.isMapleLife(cItem.ItemID) && chr.getLevel() < 30)
            {
                await _cashShopManager.EnableActions(c);
                return;
            }
        }

        // 记账在中心服，物品由频道用模板生成后放进现金仓
        await _cashShopManager.BuyCashItem(c, useNX, cItem);
    }

    /// <summary>
    /// op 0x04：赠送
    /// </summary>
    async Task HandleGift(InPacket p, IChannelClient c)
    {
        // TODO check for gender
        int birthday = p.readInt();
        var cashId = p.readInt();
        var recipientName = p.readString();
        var cItem = _cashShopManager.GetItem(cashId);
        var message = p.readString();
        if (cItem == null || !_cashShopManager.CanBuy(c, cItem, GetCash(c, CashShopServer.NX_PREPAID)) || string.IsNullOrEmpty(message) || message.Length > 73)
        {
            await _cashShopManager.EnableActions(c);
            return;
        }
        if (!c.CheckBirthday(birthday))
        {
            await c.SendPacket(PacketCreator.showCashShopMessage(0xC4));
            return;
        }
        await _cashShopManager.BuyCashItemForGift(c, CashType.NX_PREPAID, cItem, recipientName, message);
    }

    /// <summary>
    /// 余额在中心服，判断「够不够」时现查一次（真正的扣费也在中心服，这里只是提前挡掉）
    /// </summary>
    int GetCash(IChannelClient c, int cashType) => CashShopServer.GetCash(_cashShopManager.GetState(c).Cash, cashType);

    /// <summary>
    /// op 0x05：读 10 个 SN，只保留在售的
    /// </summary>
    List<int> ParseWishList(InPacket p)
    {
        var list = new List<int>(10);
        for (byte i = 0; i < 10; i++)
        {
            int sn = p.readInt();
            var cItem = _cashShopManager.GetItem(sn);
            if (cItem != null && cItem.OnSale && sn != 0)
            {
                list.Add(sn);
            }
        }
        return list;
    }
}
