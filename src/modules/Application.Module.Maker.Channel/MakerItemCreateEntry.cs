using Application.Shared.Items;

namespace Application.Module.Maker.Channel
{
    public class MakerItemCreateEntry
    {
        private int reqLevel;
        private int reqMakerLevel;
        private double cost;
        private int reqCost;
        private List<ItemQuantity> reqItems = new(); // itemId / amount
        private List<ItemQuantity> gainItems = new(); // itemId / amount

        public MakerItemCreateEntry(int cost, int reqLevel, int reqMakerLevel)
        {
            this.cost = cost;
            this.reqLevel = reqLevel;
            this.reqMakerLevel = reqMakerLevel;
        }

        public MakerItemCreateEntry(MakerItemCreateEntry mi)
        {
            this.cost = mi.cost;
            this.reqLevel = mi.reqLevel;
            this.reqMakerLevel = mi.reqMakerLevel;

            reqItems.AddRange(mi.reqItems);

            gainItems.AddRange(mi.gainItems);
        }

        public List<ItemQuantity> getReqItems()
        {
            return reqItems;
        }

        public List<ItemQuantity> getGainItems()
        {
            return gainItems;
        }

        public int getReqLevel()
        {
            return reqLevel;
        }

        public int getReqSkillLevel()
        {
            return reqMakerLevel;
        }

        public int getCost()
        {
            return reqCost;
        }

        public void addCost(double amount)
        {
            cost += amount;
        }

        public void addReqItem(int itemId, int amount)
        {
            reqItems.Add(new(itemId, amount));
        }

        public void addGainItem(int itemId, int amount)
        {
            gainItems.Add(new(itemId, amount));
        }

        public void trimCost()
        {
            reqCost = (int)(cost / 1000);
            reqCost *= 1000;
        }

        public bool isInvalid()
        {
            // thanks Rohenn, Wh1SK3Y for noticing some items not getting checked properly
            return reqLevel < 0;
        }
    }
}
