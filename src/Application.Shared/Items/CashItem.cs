namespace Application.Shared.Items
{
    public class CashItem
    {

        private int sn;
        private int itemId;
        private int price;
        public long Period { get; }
        private short count;
        private bool onSale;

        public CashItem(int sn, int itemId, int price, long period, short count, bool onSale)
        {
            this.sn = sn;
            this.itemId = itemId;
            this.price = price;
            this.Period = (period == 0 ? 90 : period);
            this.count = count;
            this.onSale = onSale;
        }

        public int getSN()
        {
            return sn;
        }

        public int getItemId()
        {
            return itemId;
        }

        public int getPrice()
        {
            return price;
        }

        public short getCount()
        {
            return count;
        }

        public bool isOnSale()
        {
            return onSale;
        }

        public bool CanBuy(int cash)
        {
            return isOnSale() && getPrice() <= cash;
        }
    }

}
