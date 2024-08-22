namespace Application.Core.Compatible.Extensions
{
    public static class HashSetExtensions
    {
        public static void addAll<TValue>(this HashSet<TValue> set, ICollection<TValue> values)
        {
            foreach (var item in values)
            {
                set.Add(item);
            }
        }
    }
}
