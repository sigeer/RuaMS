namespace Application.Core.tools.RandomUtils
{
    public class LotteryMachine<TKey>
    {
        private List<LotteryMachinItem<TKey>> _items;
        private int _sum;
        public LotteryMachine(IEnumerable<LotteryMachinItem<TKey>> items)
        {
            _items = items.OrderByDescending(x => x.Chance).ToList();
            _sum = _items.Sum(x => x.Chance);
        }

        public TKey GetRandomItem()
        {
            var value = Randomizer.nextInt(_sum);
            foreach (var item in _items)
            {
                if (value < item.Chance)
                    return item.Key;
                else
                    value -= item.Chance;
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

    public class LotteryMachine<TSource, TKey>
    {
        private readonly List<TSource> _items;
        private readonly Func<TSource, int> _chanceSelector;
        private readonly int _sum;

        public LotteryMachine(
            IEnumerable<TSource> items,
            Func<TSource, int> chanceSelector)
        {
            _items = items.ToList();
            _chanceSelector = chanceSelector ?? throw new ArgumentNullException(nameof(chanceSelector));

            _sum = _items.Sum(_chanceSelector);
            if (_sum <= 0)
                throw new ArgumentException("总概率必须大于 0", nameof(items));
        }

        public TSource GetRandomItem()
        {
            var value = Randomizer.nextInt(_sum);
            foreach (var item in _items)
            {
                var chance = _chanceSelector(item);
                if (value < chance)
                    return item;
                value -= chance;
            }
            return _items.Last();
        }
    }

}
