using tools;

namespace Application.Core.Compatible
{
    public static class Collections
    {
        public static void shuffle<TModel>(List<TModel> list)
        {
            var comparedList = new int[] { -1, 0, 1 };
            list.Sort((o1, o2) => comparedList[Randomizer.rand(0, 2)]);
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
