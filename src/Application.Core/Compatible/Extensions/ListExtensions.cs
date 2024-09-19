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



        /// <summary>
        /// 兼容js
        /// 不使用ToArray的原因：传入到js用的List，统一使用List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> toArray<T>(this List<T> list)
        {
            return list.ToList();
        }
    }
}
