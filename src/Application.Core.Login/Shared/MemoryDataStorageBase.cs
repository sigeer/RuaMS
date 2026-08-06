using Application.Core.EF;
using Application.EF;
using Application.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using ZLinq;

namespace Application.Core.Login.Shared
{
    /// <summary>
    /// 所有数据都在内存中
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class MemoryDataStorageBase<TKey, TModel, TEntity> : IDataStorage
        where TKey : notnull
        where TModel : class
        where TEntity : class, IKeyedEntity<TKey>
    {
        protected readonly IDbContextFactory<DBContext> _dbContextFactory;
        protected readonly IMapper _mapper;
        protected ConcurrentDictionary<TKey, StoreUnit<TModel>> _localData = new();
        protected TKey _localId = default!;
        protected ILogger<DataStorageBase<TKey, TModel, TEntity>> _logger;

        public StorageCategory Category { get; }

        protected MemoryDataStorageBase(StorageCategory category, IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, ILogger<DataStorageBase<TKey, TModel, TEntity>> logger)
        {
            Category = category;
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _logger = logger;
        }

        public virtual async Task InitializeAsync(DBContext dbContext)
        {
            _logger.LogInformation("正在初始化 {DataCategory}", Category);

            _localData = await SetMemoryData(dbContext);
        }

        protected async Task<ConcurrentDictionary<TKey, StoreUnit<TModel>>> SetMemoryData(DBContext dbContext)
        {
            return new((await dbContext.Set<TEntity>().ToListAsync()).Select(x => new StoreUnit<TModel>(StoreFlag.Cached, MapModel(x))).ToDictionary(x => GetKey(x.Data))); ;
        }

        protected abstract TKey GetKey(TModel model);

        void SetState(StoreFlag flag, TModel model)
        {
            var k = GetKey(model);
            if (_localData.TryGetValue(k, out var data))
            {
                data.Flag = flag;
                data.Data = model;
            }
            else
            {
                _localData[k] = new StoreUnit<TModel>(flag, model);
            }
        }

        protected virtual void SetDirty(TModel model)
        {
            SetState(StoreFlag.AddOrUpdate, model);
        }

        protected virtual void SetCache(TModel model)
        {
            SetState(StoreFlag.Cached, model);
        }

        protected virtual void SetRemoved(TModel model)
        {
            SetState(StoreFlag.Remove, model);
        }

        public virtual List<TModel> Query(Func<TModel, bool> localExpression)
        {
            return QueryLocal(localExpression);
        }

        public virtual TModel? Find(Func<TModel, bool> localExpression)
        {
            var localDataR = _localData.Values.AsValueEnumerable().Where(x => localExpression(x.Data)).FirstOrDefault();
            if (localDataR != null)
            {
                if (localDataR.Flag != StoreFlag.Remove)
                    return localDataR.Data;
                else
                    return null;
            }

            return null;
        }

        public virtual TModel? Find(TKey key)
        {
            if (_localData.TryGetValue(key, out var data))
            {
                if (data.Flag != StoreFlag.Remove)
                    return data.Data;
                else
                    return null;
            }

            return null;
        }


        protected virtual TModel MapModel(TEntity entity)
        {
            return _mapper.Map<TModel>(entity);
        }


        protected List<TModel> QueryLocal(Func<TModel, bool>? func = null)
        {
            return _localData.Values.Where(x => (func == null || func(x.Data)) && x.Flag != StoreFlag.Remove).Select(x => x.Data).ToList();
        }

        /// <summary>
        /// 保存数据库
        /// <para>从_dirty中移除保存成功的项</para>
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public virtual async Task Commit(DBContext dbContext)
        {
            _logger.LogInformation("正在保存 {DataCategory}...", Category);

            var updateData = new Dictionary<TKey, StoreUnit<TModel>>();
            foreach (var key in _localData.Keys.ToList())
            {
                if (_localData.TryGetValue(key, out var d) && d.Flag != StoreFlag.Cached)
                    updateData[key] = d;
            }

            var updateCount = updateData.Count;
            if (updateCount == 0)
            {
                _logger.LogInformation("正在保存 {DataCategory}...无修改，跳过", Category);
                return;
            }

            try
            {
                await CommitInternal(dbContext, updateData);

                _logger.LogInformation("正在保存 {DataCategory}...{Count}条", Category, updateData.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "正在保存 {DataCategory}...{Status}", Category, "失败");
                throw;
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

        protected virtual void CommitRemove(DBContext dbContext, TEntity? dbModel, TModel localModel)
        {
            if (dbModel != null)
            {
                dbContext.Set<TEntity>().Remove(dbModel);
            }
        }

        protected virtual void CommitAddOrUpdate(DBContext dbContext, TEntity? dbModel, TModel localModel)
        {
            if (dbModel == null)
            {
                dbModel = MapEntity(localModel);
                dbContext.Set<TEntity>().Add(dbModel);
            }
            else
            {
                MapExsitedEntity(localModel, dbModel);
            }
        }

        protected virtual TEntity MapEntity(TModel localModel)
        {
            return _mapper.Map<TEntity>(localModel);
        }

        protected virtual TEntity MapExsitedEntity(TModel localModel, TEntity dbModel)
        {
            return _mapper.Map(localModel, dbModel);
        }

        protected virtual async Task CommitInternal(DBContext dbContext, Dictionary<TKey, StoreUnit<TModel>> updateData)
        {
            var updatePackages = updateData.Keys.ToList();

            var allDbList = await dbContext.Set<TEntity>().Where(x => updatePackages.Contains(x.Id)).ToListAsync();
            foreach (var item in updateData)
            {
                var dbModel = allDbList.FirstOrDefault(x => x.Id.Equals(item.Key));
                if (item.Value.Flag == StoreFlag.Remove)
                {
                    CommitRemove(dbContext, dbModel, item.Value.Data);
                    continue;
                }

                CommitAddOrUpdate(dbContext, dbModel, item.Value.Data);
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
