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


using Application.Core.Channel.Net.Packets;
using Application.Core.Channel.ServerData;
using Application.Shared.Constants.Item;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/// <summary>
/// 拍卖行（MTS / ITC）内的操作
/// <para>子操作码见 docs/ida/MTS_拍卖行_封包分析.md</para>
/// </summary>
public class MTSHandler : ChannelHandlerBase
{
    readonly ILogger<MTSHandler> _logger;
    readonly MTSManager _manager;

    public MTSHandler(ILogger<MTSHandler> logger, MTSManager manager)
    {
        _logger = logger;
        _manager = manager;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;

        // 只有身处拍卖行（商城界面已打开）时才允许这些操作
        if (!chr.getCashShop().isOpened())
        {
            return;
        }

        if (p.available() == 0)
        {
            await chr.SendPacket(MTSPacketCreator.ShowMTSCash(chr.getCashShop().NxPrepaid, chr.getCashShop().MaplePoint));
            return;
        }

        byte op = p.readByte();
        switch (op)
        {
            case 2:
                // 普通上架：物品编码 + int slot + int quantity + int price + byte + byte
                await HandleSaleEntry(p, c, 14, false);
                break;
            case 3:
                // 对求购物品出价：byte + int 商品id + 物品编码 + int
                // 物品编码是变长的（装备/宠物/现金道具各不相同），这里只取商品id，
                // 交货时按求购单的物品id 从背包里取，避免依赖 GW_ItemSlotBase 的布局
                if (p.available() < 5)
                {
                    await c.SendPacket(PacketCreator.enableActions());
                    break;
                }
                p.readByte();
                await _manager.FulfillWanted(c, p.readInt(), true);
                break;
            case 4:
                {
                    // 登记求购：int itemid + int price + int quantity + byte + byte + str
                    int itemId = p.readInt();
                    int price = p.readInt();
                    int quantity = p.readInt();
                    p.readByte();
                    p.readByte();
                    if (p.available() > 0)
                    {
                        p.readString();
                    }
                    await _manager.RegisterWanted(chr, itemId, price, quantity, 0);
                    break;
                }
            case 11:
                {
                    // “我的求购”tab 的主动作（sub_59FA21，只有一个 int 商品id）。
                    // 面对自己的求购单谈不上“成交”，客户端也没给这条路准备专用的成功/失败操作码，
                    // 因此只回一次列表刷新——它带收尾字节，能把客户端“请求在途”标志清掉。
                    p.readInt();
                    await _manager.Query(chr, chr.getCurrentTab(), chr.getCurrentType(), chr.getCurrentPage());
                    break;
                }
            case 12: // 与求购单成交（int 商品id，sub_59FA94）
                await _manager.FulfillWanted(c, p.readInt(), false);
                break;
            case 13: // 求购列表：撤销（int 商品id）
                await _manager.CancelWanted(chr, p.readInt());
                break;
            case 14:
                // 全二进制 19 处 COutPacket(253) 构造点里没有 14（见 docs/ida/MTS_拍卖行_封包分析.md §4），
                // 保留分支只为在日志里暴露异常客户端
                _logger.LogWarning("收到客户端从不会构造的拍卖行操作码 14，玩家 {CharacterName}", chr.getName());
                break;
            case 18:
                // 拍卖上架：物品编码 + int slot + int quantity + int 起拍价 + int 一口价
                //           + byte 时长 + byte + int 加价幅度
                await HandleSaleEntry(p, c, 22, true);
                break;
            case 19:
                {
                    // 拍卖出价：int 商品id + int 当前价 + int 加价
                    // 客户端把“当前价”和“自己加的那部分”分两个 int 发上来（sub_5C373E），
                    // 真正的出价是两者之和；只读第一个会少掉加价部分的钱。
                    int productId = p.readInt();
                    int basePrice = p.readInt();
                    int extra = p.readInt();
                    await _manager.BidAuction(chr, productId, basePrice + extra);
                    break;
                }
            case 20:
                // 拍卖按一口价直接买下（卖家上架时填的那个价，不是服务端推算的）
                await _manager.Buy(c, p.readInt(), false);
                break;
            case 5:
                {
                    // 翻页 / 切换类型：int tab, int type, int page, byte, byte, int searchType, str search
                    int tab = p.readInt();
                    int type = p.readInt();
                    int page = p.readInt();
                    p.readByte();
                    p.readByte();
                    int searchType = p.readInt();
                    string search = p.readString();
                    await _manager.Query(chr, tab, type, page, searchType, search);
                    break;
                }
            case 6:
                {
                    // 提交搜索：int tab, int type, int page, int searchType(0=按角色名, 非0=按物品名), str keyword
                    int tab = p.readInt();
                    int type = p.readInt();
                    p.readInt();
                    int searchType = p.readInt();
                    string search = p.readString();
                    await _manager.Search(chr, tab, type, searchType, search);
                    break;
                }
            case 7:
                await _manager.CancelSaleItem(chr, p.readInt());
                break;
            case 8:
                await _manager.TakeItem(c, p.readInt());
                break;
            case 9:
                await _manager.AddCartItem(chr, p.readInt());
                break;
            case 10:
                await _manager.RemoveCartItem(chr, p.readInt());
                break;
            case 16:
                // 商品列表内直接购买（客户端先弹“Are you sure you want to buy this?”）
                await _manager.Buy(c, p.readInt(), false);
                break;
            case 17:
                // 购物车内购买
                await _manager.Buy(c, p.readInt(), true);
                break;
            default:
                _logger.LogWarning("未处理的拍卖行操作码 {Op}，玩家 {CharacterName}", op, chr.getName());
                break;
        }
    }

    /// <summary>
    /// 上架物品（op 2 普通上架 / op 18 拍卖上架）
    /// <para>
    /// 两者都是 <c>GW_ItemSlotBase::Encode(可变) + int slot + int quantity + int price + …</c>。
    /// 物品编码长度随类型不同（装备/宠物/现金道具），所以不逐字段解析，
    /// 而是从包尾固定长度反推 slot / quantity / price，只从包首取物品id。
    /// </para>
    /// </summary>
    /// <param name="tail">包尾固定字节数：op 2 是 14，op 18 是 22（多一个一口价 int、时长 byte、固定 byte、加价幅度 int）</param>
    /// <param name="auction">true 走拍卖上架（多读一口价 / 时长 / 加价幅度）</param>
    async Task HandleSaleEntry(InPacket p, IChannelClient c, int tail, bool auction)
    {
        // p 表示的是整条 payload（含已读取的 op 字节），getPosition() + available() 即总长度
        int total = p.getPosition() + p.available();
        // op(1) + itemType(1) 之后才是物品id
        if (total < 6 + tail)
        {
            await c.SendPacket(PacketCreator.enableActions());
            return;
        }

        p.seek(2);
        int itemid = p.readInt();

        p.seek(total - tail);
        short slot = (short)p.readInt();
        int quantity = p.readInt();
        int price = p.readInt();
        int durationHours = 0;
        int buyNow = 0;
        int bidIncrement = 0;
        if (auction)
        {
            // 第 4 个 int 是拍卖对话框里的“一口价”（CRegisterAuctionEntryDlg::Confirm 的第 2 个输出），
            // 与起拍价并列的两个独立输入框，不是服务端推算出来的
            buyNow = p.readInt();
            // 时长是 1 个 byte（客户端用 this[1950] <= 时长 <= this[1951] 校验 24..168）
            durationHours = p.readByte();
            // 1 个固定为 1 的 byte（Confirm 里的 *a8 = 1），跳过
            p.readByte();
            // 最后是加价幅度（Confirm 里列表控件的取值，校验 起拍价 + 加价幅度 <= 一口价）
            bidIncrement = p.readInt();
        }

        // 客户端售价区间为 110 ~ 9999999 NX（CRegisterSaleEntryDlg / CRegisterAuctionEntryDlg 校验），
        // 服务端同样限制
        if (quantity <= 0 || price < 110 || price > 9999999)
        {
            await c.SendPacket(PacketCreator.enableActions());
            return;
        }

        if (auction && (buyNow < 110 || buyNow > 9999999))
        {
            await c.SendPacket(PacketCreator.enableActions());
            return;
        }

        var invType = ItemConstants.getInventoryType(itemid);
        var item = c.OnlinedCharacter.getInventory(invType).getItem(slot);
        if (item == null || item.getItemId() != itemid)
        {
            _logger.LogWarning("玩家 {CharacterName} 尝试上架不存在或位置不符的物品 {ItemId}@{Slot}", c.OnlinedCharacter.getName(), itemid, slot);
            await c.SendPacket(PacketCreator.enableActions());
            return;
        }

        if (item.getQuantity() < quantity)
        {
            await c.SendPacket(PacketCreator.enableActions());
            return;
        }

        if (auction)
        {
            await _manager.RegisterAuction(c, invType, slot, price, buyNow, bidIncrement, durationHours);
        }
        else
        {
            await _manager.SaleItem(c, invType, slot, price);
        }
    }
}
