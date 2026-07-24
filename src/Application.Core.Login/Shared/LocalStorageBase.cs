using Application.EF;
using Application.Utility;
using System.Collections.Concurrent;
using ZLinq;

namespace Application.Core.Login.Shared
{
    /// <summary>
    /// 所有数据都在内存里
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    public abstract class LocalStorageBase<TKey, TModel> : IStorage where TKey : notnull where TModel : class
    {
        protected ConcurrentDictionary<TKey, StoreUnit<TModel>> _localData = new();
        protected TKey _localId;
        Func<TModel, TKey> _getKey;

        protected LocalStorageBase(Func<TModel, TKey> getKey)
        {
            _getKey = getKey;
        }

        public virtual async Task InitializeAsync(DBContext dbContext)
        {
            _localData = await SetLocalData(dbContext);
            _localId = await SetLocalId(dbContext);
        }

        protected abstract Task<ConcurrentDictionary<TKey, StoreUnit<TModel>>> SetLocalData(DBContext dbContext);
        protected abstract Task<TKey> SetLocalId(DBContext dbContext);

        protected abstract Task CommitInternal(DBContext dbContext, Dictionary<TKey, StoreUnit<TModel>> updateData);

        protected virtual bool SetDirty(TKey key)
        {
            if (_localData.TryGetValue(key, out var d))
            {
                d.Flag = StoreFlag.AddOrUpdate;
                return true;
            }
            return false;
        }

        protected virtual void SetDirty(TModel model)
        {
            _localData[_getKey(model)] = new StoreUnit<TModel>(StoreFlag.AddOrUpdate, model);
        }

        protected virtual bool SetRemoved(TKey key)
        {
            if (_localData.TryGetValue(key, out var d) && d.Flag != StoreFlag.Remove)
            {
                d.Flag = StoreFlag.Remove;
                return true;
            }

            _localData[key] = new StoreUnit<TModel>(StoreFlag.Remove, null);
            return true;
        }

        public virtual List<TModel> Query(Func<TModel, bool> expression)
        {
            return _localData.Values.Where(x => x.Flag != StoreFlag.Remove).Select(x => x.Data!).ToList();
        }

        /// <summary>
        /// 保存数据库
        /// <para>从_dirty中移除保存成功的项</para>
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public virtual async Task Commit(DBContext dbContext)
        {
            var updateData = new Dictionary<TKey, StoreUnit<TModel>>();
            foreach (var key in _localData.Keys.ToList())
            {
                if (_localData.TryGetValue(key, out var d) && d.Flag != StoreFlag.Cached)
                    updateData[key] = d;
            }

            var updateCount = updateData.Count;
            if (updateCount == 0)
                return;

            try
            {
                await CommitInternal(dbContext, updateData);
            }
            finally
            {
                foreach (var kw in updateData)
                {
                    if (kw.Value.Flag == StoreFlag.Remove)
                    {
                        _localData.TryRemove(kw.Key, out _);
                    }
                    else
                    {
                        _localData.GetValueOrDefault(kw.Key)?.Flag = StoreFlag.Cached;
                    }
                }
            }
        }
    }
}
