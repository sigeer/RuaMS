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
        /// </summary>
        public static Packet ShowMTSCash(int nxPrepaid, int maplePoint)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION2);
            p.writeInt(nxPrepaid);
            p.writeInt(maplePoint);
            return p;
        }

        /// <summary>
        /// 上架数量已达上限：op 61 (0x3D)
        /// </summary>
        public static Packet MTSWantedListingOver(int nx, int items)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(61);
            p.writeInt(nx);
            p.writeInt(items);
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
        /// 带原因的操作失败：op 38 (0x26) + 一个 error code
        /// <para>
        /// 客户端 <c>sub_5A4D49</c>（0x400000 基址）读一个 byte 后弹对应提示；
        /// 同族的 op 34 / op 36 在 code 为 73 时还会把玩家送出拍卖行，op 38 不会。
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
        /// </summary>
        public static Packet MTSConfirmTransfer(int quantity, int pos)
        {
            OutPacket p = OutPacket.create(SendOpcode.MTS_OPERATION);
            p.writeByte(39);
            p.writeInt(quantity);
            p.writeInt(pos);
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
            var item = ProtoMapper.MapItem(src.Item);
            if (item == null)
            {
                return;
            }

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
        public static Packet MTSWantedOffered(bool fromOffer, bool success)
        {
            if (fromOffer)
                return Empty(success ? 31 : 32);
            return Empty(success ? 47 : 48);
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
