using Application.EF;
using System.Collections.Concurrent;

namespace Application.Core.Login.Shared
{
    public abstract class StorageBase<TKey, TModel> : IStorage where TKey : notnull
    {
        ConcurrentDictionary<TKey, TModel> _dirty = new();

        protected virtual Task CommitInternal(DBContext dbContext, Dictionary<TKey, TModel> updateData)
        {
            return Task.CompletedTask;
        }

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

            await CommitInternal(dbContext, updateData);
        }
    }
}
