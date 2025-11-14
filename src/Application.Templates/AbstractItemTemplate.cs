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
}