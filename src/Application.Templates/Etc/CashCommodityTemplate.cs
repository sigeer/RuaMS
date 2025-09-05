namespace Application.Templates.Etc
{
    public class CashCommodityTemplate : AbstractTemplate
    {
        public int Gender { get; set; }
        public int Count { get; set; }
        public int Period { get; set; }
        public int Priority { get; set; }
        /**
         * SN is a number used by Nexon to identify cash items.
         * Cash items have an item id like other items, and also a serial number.
         * When the client communicates with the server about the purchase and rearranging 
         *  of cash items (except in the wish list), the serial number is used.
         *  In all other instances the item id is used (equipping, using items, etc).
         */
        public long CashItemSN { get; set; }
        public int ItemID { get; set; }
        public int Price { get; set; }
        public bool OnSale { get; set; }
        public int Classification { get; set; }

        public CashCommodityTemplate(int templateId)
            : base(templateId) { }
    }
}
