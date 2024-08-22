using System.Collections.Concurrent;

namespace Application.Core.Compatible.Extensions
{
    public static class ConcurrentExtensions
    {
        public static void Remove<TValue>(this ConcurrentBag<TValue> list, TValue item) where TValue : class
        {
            while (list.TryTake(out var d))
            {
                if (d != item)
                {
                    list.Add(d);
                }
                else
                {
                    break;
                }
            }
        }
    }
}
