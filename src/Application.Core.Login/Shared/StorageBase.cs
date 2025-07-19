using Application.EF;
using Application.Utility;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using ZLinq;

namespace Application.Core.Login.Shared
{
    public abstract class StorageBase<TKey, TModel> : IStorage where TKey : notnull where TModel : class, ITrackableEntityKey<TKey>
    {
        protected ConcurrentDictionary<TKey, StoreUnit<TModel>> _localData = new();

        protected abstract Task CommitInternal(DBContext dbContext, Dictionary<TKey, StoreUnit<TModel>> updateData);

        protected virtual bool SetDirty(TKey key, StoreUnit<TModel> model)
        {
            if (_localData.TryGetValue(key, out var d))
            {
                _localData[key] = model;
                return d.Flag != d.Flag;
            }

            _localData[key] = model;
            return true;
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
            Dictionary<TKey, TModel> sourceDict = dataFromDB.ToDictionary(x => x.Id);

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
                if (_localData.TryRemove(key, out var d) && d.Flag != StoreFlag.Cached)
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
                    _localData.TryAdd(item.Key, item.Value);
                }
                throw;
            }
        }
    }
}
