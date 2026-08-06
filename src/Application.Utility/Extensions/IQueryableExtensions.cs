using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Utility.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ToPage<T>(this IQueryable<T> list, int index, int count)
        {
            index = Math.Max(1, index);
            return count < 1 ? list : list.Skip((index - 1) * count).Take(count);
        }
    }

    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> ToPage<T>(this IEnumerable<T> list, int index, int count)
        {
            index = Math.Max(1, index);
            return count < 1 ? list : list.Skip((index - 1) * count).Take(count);
        }
    }
}
