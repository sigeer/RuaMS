namespace Application.Templates
{
    public abstract class AbstractItemTemplate : AbstractTemplate
    {
        [WZPath("info/accountSharable")]
        public bool AccountSharable { get; set; }
        [WZPath("info/tradeAvailable")]
        public bool TradeAvailable { get; set; }
        [WZPath("info/slotMax")]
        public virtual int SlotMax { get; set; } = 200;
        [WZPath("info/cash")]
        public bool Cash { get; set; }

        /// <summary>
        /// 不可交易
        /// </summary>
        [WZPath("info/tradeBlock")]
        public bool TradeBlock { get; set; }

        [WZPath("info/notSale")]
        public bool NotSale { get; set; }

        [WZPath("info/quest")]
        public bool Quest { get; set; }

        [WZPath("info/only")]
        public bool Only { get; set; }

        [WZPath("info/price")]
        public int Price { get; set; } = -1;


        [WZPath("info/expireOnLogout")]
        public bool ExpireOnLogout { get; set; }

        [WZPath("info/timeLimited")]
        public bool TimeLimited { get; set; }


        [WZPath("spec/time")]
        public virtual int Time { get; set; } = -1;

        [WZPath("info/replace")]
        public ReplaceItemTemplate? ReplaceItem { get; set; }


        protected AbstractItemTemplate(int templateId)
            : base(templateId)
        {
        }
    }

    public sealed class ReplaceItemTemplate
    {
        [WZPath("info/replace/itemid")]
        public int ItemId { get; set; }
        [WZPath("info/replace/msg")]
        public string Message { get; set; } = string.Empty;
        public int Period { get; set; }
    }

    public sealed class LotteryItemTemplate : AbstractItemTemplate
    {
        /*
		 * 00000000 CItemInfo::LOTTERYITEM struc ; (sizeof=0x8, align=0x4, copyof_5727)
		   00000000 aEntity ZArray<CItemInfo::LOTTERY_ENTITY> ?
		   00000004 nTotalProb dd ?
		   00000008 CItemInfo::LOTTERYITEM ends
		 */

        public LotteryEntity[] Entity { get; set; }
        public int TotalProb { get; set; }

        public LotteryItemTemplate(int templateId) : base(templateId)
        {

        }

        public sealed class LotteryEntity
        {
            /*
			 * 00000000 CItemInfo::LOTTERY_ENTITY struc ; (sizeof=0x1C, align=0x4, copyof_5725)
			   00000000 nItemID dd ?
			   00000004 nProb dd ?
			   00000008 nQuantity dd ?
			   0000000C sEffect ZXString<char> ?
			   00000010 sWorldMsg ZXString<char> ?
			   00000014 nPeriod dd ?
			   00000018 sDateExpire ZXString<char> ?
			   0000001C CItemInfo::LOTTERY_ENTITY ends
			 */

            public int Prob { get; set; }
            public int Quantity { get; set; }
            public string Effect { get; set; }
            public string WorldMsg { get; set; }
            public int Period { get; set; }
            public DateTime DateExpire { get; set; }
        }
    }
}