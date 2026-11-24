namespace Application.Shared.Items
{
    /// <summary>
    /// 拍卖行（MTS / ITC）的常量与费率
    /// <para>
    /// 中心服（<c>Core.Login</c>）和频道服（<c>Core</c>）用的是同一套数值，统一放在这里，
    /// 免得两边各留一份副本之后失步。
    /// </para>
    /// </summary>
    public static class MTSConstants
    {
        /// <summary>
        /// 上架手续费（金币）
        /// </summary>
        public const int SALE_FEE = 5000;

        /// <summary>
        /// 出售列表所在 tab（客户端 <c>CITCWnd_List</c> 的 tab 号，见 docs/ida/MTS_拍卖行_封包分析.md）
        /// </summary>
        public const int SALE_TAB = 1;
        /// <summary>
        /// 求购列表所在 tab
        /// </summary>
        public const int WANTED_TAB = 2;
        /// <summary>
        /// 拍卖列表所在 tab
        /// </summary>
        public const int AUCTION_TAB = 3;
        /// <summary>
        /// 购物车所在 tab
        /// </summary>
        public const int CART_TAB = 4;
        /// <summary>
        /// “我的求购”所在 tab（与购物车同一个 tab，用 type 区分）
        /// </summary>
        public const int MY_WANTED_TAB = 4;
        /// <summary>
        /// “我的求购”使用的 type
        /// </summary>
        public const int MY_WANTED_TYPE = 1;

        /// <summary>
        /// 每页商品数，与客户端写死的 16 一致
        /// </summary>
        public const int PAGE_SIZE = 16;

        /// <summary>
        /// 每个角色最多同时上架的商品数
        /// </summary>
        public const int MAX_SALE_ITEMS = 10;

        /// <summary>
        /// 拍卖时长下限（小时），随 SET_ITC 下发的 1950
        /// </summary>
        public const int AUCTION_MIN_HOURS = 24;
        /// <summary>
        /// 拍卖时长上限（小时），随 SET_ITC 下发的 1951
        /// </summary>
        public const int AUCTION_MAX_HOURS = 168;

        /// <summary>
        /// 买家实付的加价百分比，随 SET_ITC 下发的 1948
        /// <para>
        /// 客户端算买家实付总额的式子是 <c>1949 + (1948+100) × 价格 / 100</c>
        /// （<c>sub_5C3947</c> 的出价余额校验、<c>sub_5BC1D5</c> case 3 的一口价确认框），
        /// 所以这里的值必须和实际下发的 1948 一致，否则确认框显示的金额和扣款对不上。
        /// </para>
        /// </summary>
        public const int Fee_Rate = 7;
        /// <summary>
        /// 买家实付的固定加价，随 SET_ITC 下发的 1949
        /// </summary>
        public const int Fee_Base = 500;

        /// <summary>
        /// 普通出售 / 求购单的上架有效期
        /// </summary>
        public static readonly TimeSpan SALE_DURATION = TimeSpan.FromDays(1);

        /// <summary>
        /// 税费 = <c>Fee_Base + 价格 × Fee_Rate / 100</c>，当前取值即 <c>500 + 价格 × 7%</c>。
        /// <para>
        /// 两处都用它：
        /// </para>
        /// <list type="bullet">
        /// <item>条目第 3 段（<c>+0x24</c>）下发直售/求购的税费，
        /// 客户端 tooltip 的总价就是「税费 + 售价」—— 这一段是服务端说了算的，见
        /// <c>MTSPacketCreator.writeMTSItemEntry</c>；</item>
        /// <item>拍卖的买家实付（<see cref="BuyerTotalCost"/>）。</item>
        /// </list>
        /// </summary>
        public static int GetTaxes(int price) => Fee_Base + price * Fee_Rate / 100;

        /// <summary>
        /// 买家实付 = 成交价 + 税费 = <c>成交价 + 500 + 成交价 × 7%</c>。
        /// <para>
        /// 与客户端确认框的式子在整数除法下逐位相等：
        /// </para>
        /// <code>
        /// 客户端：1949 + (1948+100) × 价格 / 100 = 500 + 107 × 价格 / 100
        /// 本方法：价格 + (500 + 价格 × 7 / 100) = 500 + 价格 + 价格 × 7 / 100
        /// （107 × 价格 / 100 ≡ 价格 + 价格 × 7 / 100）
        /// </code>
        /// <para>
        /// <b>例外：拍卖行列表上「BUY NOW」那一行是客户端写死的。</b>
        /// <c>sub_8F6DC9</c> 在那里直接算 <c>一口价 + 一口价/10 + 100</c>
        /// （<c>push 0Ah</c> / <c>lea eax,[eax+edx+64h]</c>），整个函数 4854 条指令里
        /// 没有任何一处读取 <c>+0x1E70</c>（1948）或 <c>+0x1E74</c>（1949）——
        /// 也就是说它压根没走 SET_ITC 的参数。所以在 7 / 500 这套取值下，
        /// 列表显示的买断价（<c>一口价 + 10% + 100</c>）会比确认框低，两者无法同时对齐：
        /// 要让列表也对，只能把 1948 / 1949 发成 10 / 100，但那样确认框又会变。
        /// 服务端以<b>确认框</b>为准 —— 它是购买前最后告知玩家、并且参与余额校验的数字。
        /// </para>
        /// <para>
        /// 这个式子不是一口价：一口价是卖家上架时在 <c>CRegisterAuctionEntryDlg</c> 里
        /// 单独填的另一个价（客户端条目 +0x108），卖家按成交价入账。
        /// </para>
        /// </summary>
        public static int BuyerTotalCost(int dealPrice) => dealPrice + GetTaxes(dealPrice);
    }
}
