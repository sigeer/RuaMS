namespace Application.Utility.Extensions
{
    public static class ListExtensions
    {
        public static TValue remove<TValue>(this IList<TValue> list, int index)
        {
            var item = list[index];
            list.RemoveAt(index);
            return item;
        }

        public static TValue get<TValue>(this List<TValue> list, int index)
        {
            return list[index];
        }
    }
}
