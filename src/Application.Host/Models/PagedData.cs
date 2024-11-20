using Microsoft.EntityFrameworkCore;

namespace Application.Host.Models
{
    public class PagedData<TData>
    {
        public List<TData> Data { get; set; } = [];
        public int Total { get; set; }
    }

    public class Pagination
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public static class PagedExtensions
    {
        public static PagedData<TModel> ToPage<TModel>(this IQueryable<TModel> dbSet, int pageIndex, int pageSize)
        {
            if (pageSize < 0)
            {
                var dataList = dbSet.ToList();
                return new PagedData<TModel>() { Total = dataList.Count, Data = dataList };
            }
            if (pageSize == 0)
                return new PagedData<TModel>() { Total = dbSet.Count() };

            pageIndex = pageIndex > 0 ? pageIndex : 1;

            var total = dbSet.Count();
            var list = dbSet.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PagedData<TModel> { Data = list, Total = total };
        }

        public static async Task<PagedData<TModel>> ToPageAsync<TModel>(this IQueryable<TModel> dbSet, int pageIndex, int pageSize)
        {
            if (pageSize < 0)
            {
                var dataList = await dbSet.ToListAsync();
                return new PagedData<TModel>() { Total = dataList.Count, Data = dataList };
            }
            if (pageSize == 0)
                return new PagedData<TModel>() { Total = await dbSet.CountAsync() };

            pageIndex = pageIndex > 0 ? pageIndex : 1;

            var total = await dbSet.CountAsync();
            var list = await dbSet.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedData<TModel> { Data = list, Total = total };
        }
    }
}
