using Application.Core.Client;
using Application.Core.Mappers;
using Application.Shared.Items;
using Application.Shared.Net;
using tools;

namespace Application.Core.Channel.Net.Packets
{
    /// <summary>
    /// 拍卖行（MTS / ITC）的封包构造
    /// <para>封包结构由客户端 CITC 系列窗口决定，字段顺序不可更改</para>
    /// </summary>
    public static class MTSPacketCreator
    {
        /// <summary>
        /// 打开拍卖行界面（SET_ITC）
        /// </summary>
        public static Packet OpenMTS(IChannelClient c)
        {
            return PacketCreator.openCashShop(c, true);
        }

        /// <summary>
        /// 商品列表：op 21 (0x15)
        /// </summary>
        public static Packet SendMTS(List<ProtoModel.MTSItemProto> items, int tab, int type, int page, int pages)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(21);
            p.writeInt(pages * 16);
            p.writeInt(items.Count);
            p.writeInt(tab);
            p.writeInt(type);
            p.writeInt(page);
            p.writeByte(1);
            p.writeByte(1);

            foreach (var item in items)
            {
                writeMTSItemEntry(p, item);
            }

            p.writeByte(1);
            return p;
        }

        /// <summary>
        /// 拍卖行内的货币：op MTS_OPERATION2，两个 int（抵用券 + 抵用券）
        /// <para>
        /// <b>只给拍卖行用</b>（`MTSManager`）。商城界面读的是 `QUERY_CASH_RESULT`
        /// （<see cref="PacketCreator.showCash"/>），发错封包客户端不会刷新余额。
        /// </para>
        /// </summary>
        public static Packet ShowMTSCash(int nxPrepaid, int maplePoint)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION2);
            p.writeInt(nxPrepaid);
            p.writeInt(maplePoint);
            return p;
        }

        /// <summary>
        /// 求购/挂单到期：op 61 (0x3D)
        /// <para>
        /// 客户端 <c>sub_5A523E</c>：<c>v1 = Decode4(); v2 = Decode4();
        /// if (v1 &gt;= 0 &amp;&amp; v2 &gt; 0) 提示 SP_4815 …</c>，
        /// 文案里只代入 <c>v2</c>（到期条数），<c>v1</c> 只参与「是否提示」的判断。
        /// </para>
        /// </summary>
        public static Packet MTSWantedListingOver(int reserved, int expiredCount)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(61);
            p.writeInt(reserved);
            p.writeInt(expiredCount);
            return p;
        }

        /// <summary>
        /// 某件物品成交提示：op 62 (0x3E)
        /// <para>
        /// 客户端 <c>sub_5A52DE</c> 依次读：
        /// <c>byte(1 = 卖出 / 其它 = 买入)</c> → <c>int itemId</c> → <c>int nx</c> →
        /// <c>DecodeBuffer(8)</c>（成交时间 FILETIME）。
        /// 文案：卖出 <c>SP_4761_..._IS_SOLD_FOR_..._NX</c>，买入 <c>SP_4762_..._IS_BOUGHT_FOR_..._NX</c>。
        /// </para>
        /// </summary>
        public static Packet MTSItemSold(bool sold, int itemId, int nx, DateTimeOffset time)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(62);
            p.writeByte(sold ? 1 : 0);
            p.writeInt(itemId);
            p.writeInt(nx);
            p.writeLong(PacketCommon.getTime(time.ToUnixTimeMilliseconds()));
            return p;
        }

        /// <summary>
        /// 通用结果码：op 30 (0x1E)
        /// <para>
        /// 客户端 <c>sub_5A4581</c> 先 <c>Decode1</c> 取结果码并清「请求在途」标志，再按码分支：
        /// </para>
        /// <list type="bullet">
        /// <item><b>72</b>：额外 <c>Decode2</c> 读一个 short，提示
        /// <c>SP_4778_LIMIT_TO_D_UNITS_OR_LESS</c>（最多 N 个单位），见 <see cref="MTSLimitUnits"/>；</item>
        /// <item><b>63 / 68</b>：弹对应提示并 <c>CITC::SendTransferFieldPacket</c> 强制离场；</item>
        /// <item><b>66 'B' / 67 'C' / 71 'G' / 78 'N' / 79 'O' / 81 'Q' / 82 'R' / 83 'S'</b>：
        /// 走 <c>sub_5A4752</c> 的文案表（NX 不足 / 金币不足 / 背包满 / 有出价不能撤 / 不可出售 /
        /// 已售出不能取消 / 飞镖不可交易 / 未满 10 级），只弹提示；</item>
        /// <item>其余码（含 0）：弹兜底文案 <c>SP_4808_DUE_TO_AN_UNKNOWN_ERROR...</c>。</item>
        /// </list>
        /// <para>
        /// 与 op 38 的区别：op 30 在 63 / 68 时会额外把玩家送出拍卖行，op 38 不会；
        /// 需要「失败并离场」的场景用 op 30 的 68（'D'，NX 异常）。
        /// </para>
        /// </summary>
        public static Packet MTSOperationResult(MTSOperationCode code)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(30);
            p.writeByte((byte)code);
            return p;
        }

        /// <summary>
        /// 数量超过上限：op 30 (0x1E) + 结果码 72 + 上限（short）
        /// <para>
        /// 客户端提示 <c>SP_4778_LIMIT_TO_D_UNITS_OR_LESS</c>（最多 N 个单位）。
        /// 注意语义是<b>单次交易的数量上限</b>，不是「最多同时上架 N 件」——后者客户端没有专用包。
        /// </para>
        /// </summary>
        public static Packet MTSLimitUnits(short limit)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(30);
            p.writeByte((byte)MTSOperationCode.LimitUnits);
            p.writeShort(limit);
            return p;
        }

        /// <summary>
        /// 上架成功：op 29 (0x1D)
        /// </summary>
        public static Packet MTSConfirmSell()
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(29);
            return p;
        }

        /// <summary>
        /// 购买成功：op 51 (0x33)
        /// </summary>
        public static Packet MTSConfirmBuy()
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(51);
            return p;
        }

        /// <summary>
        /// 购买失败：op 52 (0x34)
        /// </summary>
        public static Packet MTSFailBuy()
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(52);
            return p;
        }

        /// <summary>
        /// 带原因的操作失败：op 38 (0x26) + 一个 <see cref="MTSClientErrorCode"/>
        /// <para>
        /// 这一套码（op 34 / 36 / 38）与 op 30 的 <see cref="MTSOperationCode"/> <b>不同</b>：
        /// 这里任意 byte 都会走 <c>sub_5A4752</c> 的文案表，客户端总会弹提示；
        /// op 30 只认固定几个码，且 63 / 68 会强制离场、72 还要再跟 short。
        /// </para>
        /// <para>
        /// 客户端 <c>sub_5A4D49</c>（0x400000 基址）读一个 byte 后弹对应提示；
        /// 同族的 op 34 (<c>sub_5A4C2A</c>) / op 36 (<c>sub_5A4CE7</c>) 在 code 为 73 时
        /// 还会调用 <c>CITC::SendTransferFieldPacket</c> 把玩家送出拍卖行，op 38 不会。
        /// 需要「强制离场」的失败场景走 op 34 / 36，普通失败走 op 38。
        /// </para>
        /// </summary>
        public static Packet MTSFailError(MTSClientErrorCode code)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(38);
            p.writeByte((byte)code);
            return p;
        }

        /// <summary>
        /// 物品已放入背包：op 39 (0x27)
        /// <para>
        /// 客户端 <c>sub_5A4D68</c>：<c>v1 = Decode4(); v2 = Decode4();
        /// CCtrlTab::SetTab(v1 - 1); sub_5B3B00(v2);</c> 然后提示
        /// <c>SP_4787_THE_ITEM_HAS_BEEN_TRANSFERRED</c>。
        /// 所以第一个 int 是<b>1 基背包页</b>（装备=1 … 现金=5），不是数量；
        /// 第二个 int 是落点格子。旧实现按 Java 版传 <c>item.getQuantity()</c> 是错的。
        /// </para>
        /// <para>
        /// <b>这个包不往背包里放东西</b>。<c>sub_5B3B00</c> 只是在客户端已缓存的那一页里
        /// 选中格子、把行滚进可视区（详见 docs/ida/MTS_拍卖行_封包分析.md §13.2）。
        /// 物品本体必须先用 <see cref="PacketCreator.InventoryOperation"/> 的
        /// <c>InventoryAdd</c> 下发，否则客户端选中的是空格。
        /// </para>
        /// </summary>
        public static Packet MTSConfirmTransfer(int page, int slot)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(39);
            p.writeInt(page);
            p.writeInt(slot);
            return p;
        }

        /// <summary>
        /// 我上架中（未售出）的商品：op 35 (0x23)
        /// </summary>
        public static Packet NotYetSoldInv(List<ProtoModel.MTSItemProto> items)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(35);
            p.writeInt(items.Count);
            foreach (var item in items)
            {
                writeMTSItemEntry(p, item);
            }
            return p;
        }

        /// <summary>
        /// 待领取列表：op 33 (0x21)
        /// <para>尾部是一个 int（未能显示的超额数量）和一个 byte（结束标记），见 sub_5A4AF3</para>
        /// </summary>
        public static Packet TransferInventory(List<ProtoModel.MTSItemProto> items)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(33);
            p.writeInt(items.Count);
            foreach (var item in items)
            {
                writeMTSItemEntry(p, item);
            }
            p.writeInt(0);
            p.writeByte(1);
            return p;
        }

        /// <summary>
        /// 单个商品条目，客户端 CITC 的 <c>CITCItemEntry</c> 解析函数按固定顺序读取
        /// <para>顺序与长度由客户端 0x5A2C0F 反编译得到，见 docs/ida/MTS_拍卖行_封包分析.md</para>
        /// </summary>
        private static void writeMTSItemEntry(OutPacket p, ProtoModel.MTSItemProto src)
        {
            // 条目数是先写出去的，这里少写一条客户端就会整页读串位；
            // 模板缺失由 MapItem 直接抛异常，整条包构建失败，不会发出错位数据
            var item = ProtoMapper.MapItem(src.Item);
            var isAuction = src.Tab == MTSConstants.AUCTION_TAB;

            // 1. 物品本体
            PacketCreator.addItemInfo(p, item, true);

            // 2~4. 商品id、税费、售价
            p.writeInt(src.Id);
            p.writeInt(MTSConstants.GetTaxes(src.Price));
            // 第 4 段（+0xFC）：客户端在拍卖 tab 用它算「当前最高出价 = 第 13 段 + 第 4 段」
            // （sub_8F6DC9 的 0x8F7A4C），同时「单价 = 第 4 段/数量 + 第 16 段」。
            // 出售 / 求购条目填售价（物品 tooltip 的总价 = 税费 + 本段）；
            // 拍卖条目**必须填 0** —— 第 13 段已经是当前价，再填一次会显示成两倍。
            p.writeInt(isAuction ? 0 : src.Price);

            // 5~6. 出售者账号名、角色名
            p.writeString(src.SellerName);
            p.writeString(src.SellerName);

            // 7. 上架结束时间（8 字节 FILETIME）
            p.writeLong(PacketCommon.getTime(src.SellEnds));

            // 8~10. 三个 50 字节定长字符串。
            // 第 8 段（+0x34）不是预留：客户端 sub_5BC1D5 的四个 tab 分支都用
            // dword_BF03C4(条目+0x34, 本机玩家名) 判断“这条是不是我自己挂的”，
            // 写空串会导致自己的挂单被当成别人的（能加购物车、点买断），必须填出售者名字。
            p.writeString(src.SellerName);
            p.writeString(string.Empty);
            p.writeString(string.Empty);

            // 11~12. 客户端按 Decode4 读、只保留低 16 位。
            // 第 11 段（+0x0C）是**出价人数**：sub_8F6DC9 的 0x8F7775 把 LOWORD(entry+12)
            // 拼进 SP_4766_BID_COUNT__D；第 12 段（+0x0E）是加价幅度，出价对话框按它步进。
            p.writeInt(src.BidCount);
            p.writeInt(src.BidIncrement);

            // 13~16. 第 13 段（+0x10）是价格本体：出售/求购是售价，拍卖是当前最高价
            // （出价对话框也拿它当基准，见 sub_5C28F9）；
            // 第 15 段（+0x108）是拍卖的一口价 —— 卖家上架时自己填的那个，客户端显示时会自己加税费；
            // 第 16 段（+0x10C）是拍卖的「单价」组成项（SP_4763_PRICE_PER_UNIT__D）。
            p.writeInt(src.Price);
            p.writeInt(0);
            p.writeInt(src.BuyNow);

            var quantity = (int)item.getQuantity();
            p.writeInt(isAuction ? src.Price / Math.Max(1, quantity) : 0);

            // 17. 结尾 short
            p.writeShort(0);
        }

        /// <summary>
        /// 求购登记结果：成功 op 55 (0x37) / 失败 op 56 (0x38)
        /// </summary>
        public static Packet MTSWantedRegistered(bool success) => Empty(success ? 55 : 56);

        /// <summary>
        /// 与求购单成交结果。
        /// <para>
        /// 两条入口用两套操作码：op 3（客户端带物品编码）走 31/32，op 11 / op 12 走 47/48，
        /// 见 docs/ida/MTS_拍卖行_封包分析.md §6。
        /// </para>
        /// </summary>
        public static Packet MTSWantedOffered(bool fromOffer, bool success, byte reason = 0)
        {
            if (fromOffer)
                return success ? Empty(31) : MTSWantedOfferedFailed(reason);
            return Empty(success ? 47 : 48);
        }

        /// <summary>
        /// 对求购单出价失败：op 32 (0x20)，**带一个 byte 原因码**
        /// <para>
        /// 客户端 <c>sub_5A46F0</c>：<c>code == 80</c> → <c>SP_4806_THE_ITEM_IS_CANCELLED_OR_SOLD_OUT</c>；
        /// <c>code == 82</c> → <c>SP_4816_THROWING_STARS_ARE_NOT_TRADABLE</c>；
        /// 其它 → <c>SP_4782_REQUESTING_THE_ITEM_FOR_SALE_FAILED</c>。
        /// 之前发的是空包，客户端会去读一个不存在的字节。
        /// </para>
        /// </summary>
        public static Packet MTSWantedOfferedFailed(byte reason)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(32);
            p.writeByte(reason);
            return p;
        }

        /// <summary>
        /// 撤销求购结果：成功 op 49 (0x31) / 失败 op 50 (0x32)
        /// </summary>
        public static Packet MTSWantedCanceled(bool success) => Empty(success ? 49 : 50);

        /// <summary>
        /// 加入购物车结果：成功 op 41 (0x29) / 失败 op 42 (0x2A)
        /// </summary>
        public static Packet MTSCartAdded(bool success) => Empty(success ? 41 : 42);

        /// <summary>
        /// 移出购物车结果：成功 op 43 (0x2B) / 失败 op 44 (0x2C)
        /// </summary>
        public static Packet MTSCartRemoved(bool success) => Empty(success ? 43 : 44);

        /// <summary>
        /// 取消出售成功：op 37 (0x25)
        /// </summary>
        public static Packet MTSCancelSell() => Empty(37);

        /// <summary>
        /// 不能连续出价：op 60 (0x3C)，客户端文案
        /// <c>YOU_CANNOT_MAKE_A_CONSECUTIVE_BID</c>
        /// </summary>
        public static Packet MTSConsecutiveBid() => Empty(60);

        static Packet Empty(int op)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(op);
            return p;
        }
    }
}
