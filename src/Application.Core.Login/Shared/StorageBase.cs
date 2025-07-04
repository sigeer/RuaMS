using Application.EF;
using System.Collections.Concurrent;

namespace Application.Core.Login.Shared
{
    public abstract class StorageBase<TKey, TModel> : IStorage where TKey : notnull
    {
        protected ConcurrentDictionary<TKey, TModel> _dirty = new();

        protected abstract Task CommitInternal(DBContext dbContext, Dictionary<TKey, TModel> updateData);

        protected virtual void SetDirty(TKey key, TModel model)
        {
            _dirty[key] = model;
        }

        public virtual async Task Commit(DBContext dbContext)
        {
            var updateData = new Dictionary<TKey, TModel>();
            foreach (var key in _dirty.Keys.ToList())
            {
                if (_dirty.TryRemove(key, out var d))
                    updateData[key] = d;
            }

            var updateCount = updateData.Count;
            if (updateCount == 0)
                return;

            try
            {
                await CommitInternal(dbContext, updateData);
            }
            catch (Exception)
            {
                foreach (var item in updateData)
                {
                    _dirty.TryAdd(item.Key, item.Value);
                }
                throw;
            }
        }
    }
}
