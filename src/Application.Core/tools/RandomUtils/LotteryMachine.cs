using tools;

namespace Application.Core.tools.RandomUtils
{
    public class LotteryMachine<TKey>
    {
        private List<LotteryMachinItem<TKey>> _items;

        public LotteryMachine(IEnumerable<LotteryMachinItem<TKey>> items)
        {
            _items = items.OrderByDescending(x => x.Chance).ToList();
        }

        public TKey GetRandomItem()
        {
            var sum = (double)_items.Sum(x => x.Chance);

            var value = Randomizer.nextDouble();
            foreach (var item in _items)
            {
                if (value < item.Chance)
                    return item.Key;
                else
                    value -= item.Chance / sum;
            }
            return _items.Last().Key;
        }
    }

    public class LotteryMachinItem<TKey>
    {
        public LotteryMachinItem(TKey key, int chance)
        {
            this.Key = key;
            this.Chance = chance;
        }

        public TKey Key { get; set; }
        public int Chance { get; set; }
    }
}
