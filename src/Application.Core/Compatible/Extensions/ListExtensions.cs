namespace Application.Core.Compatible.Extensions
{
    public static class ListExtensions
    {
        public static TValue remove<TValue>(this IList<TValue> list, int index)
        {
            var item = list[index];
            list.RemoveAt(index);
            return item;
        }

        public static void set<TValue>(this List<TValue> list, int index, TValue value)
        {
            list[index] = value;
        }

        public static TValue get<TValue>(this List<TValue> list, int index)
        {
            return list[index];
        }
    }
}
