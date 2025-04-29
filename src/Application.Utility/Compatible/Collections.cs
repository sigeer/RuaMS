
namespace Application.Utility.Compatible
{
    public static class Collections
    {
        public static void shuffle<TModel>(List<TModel> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Randomizer.nextInt(n + 1);  // 生成 0 到 n 之间的随机索引
                (list[n], list[k]) = (list[k], list[n]);  // 交换元素
            }
        }


        public static List<TType> singletonList<TType>(params TType[] list)
        {
            return new List<TType>(list);
        }

        public static void fill<T>(List<T> accHist, T blockExpiration)
        {
            for (int i = 0; i < accHist.Count; i++)
            {
                accHist[i] = blockExpiration;
            }
        }

        public static Dictionary<TKey, TValue> singletonMap<TKey, TValue>(TKey k, TValue v) where TKey : notnull
        {
            return new Dictionary<TKey, TValue>() { { k, v } };
        }
    }
}
