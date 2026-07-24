using Application.EF;
using Application.EF.Entities;
using Application.Utility;
using DueyDto;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using ZLinq;

namespace Application.Core.Login.Shared
{
    /// <summary>
    /// 部分数据在内存里
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    public abstract class StorageBase<TKey, TModel> : IStorage where TKey : notnull where TModel : class
    {
        protected ConcurrentDictionary<TKey, StoreUnit<TModel>> _localData = new();
        Func<TModel, TKey> _getKey;

        protected StorageBase(Func<TModel, TKey> getKey)
        {
            _getKey = getKey;
        }

        public abstract Task InitializeAsync(DBContext dbContext);

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

        public abstract List<TModel> Query(Expression<Func<TModel, bool>> expression);

        /// <summary>
        /// 查询，数据库数据 + 修改过的数据 - 移除的数据
        /// </summary>
        /// <param name="dataFromDB">来自数据库的数据</param>
        /// <param name="func">内存中的数据</param>
        /// <returns></returns>
        protected List<TModel> QueryWithDirty(List<TModel> dataFromDB, Func<TModel, bool> func)
        {
            Dictionary<TKey, TModel> sourceDict = dataFromDB.ToDictionary(x => _getKey(x));

            foreach (var kv in _localData)
            {
                var entry = kv.Value;
                if (entry.Flag == StoreFlag.AddOrUpdate && func(entry.Data!))
                {
                    sourceDict[kv.Key] = entry.Data!;
                }
                else if (entry.Flag == StoreFlag.Remove)
                {
                    sourceDict.Remove(kv.Key);
                }
            }

            return sourceDict.Values.ToList();
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
