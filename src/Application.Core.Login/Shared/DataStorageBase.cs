using Application.Core.EF;
using Application.EF;
using Application.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using ZLinq;

namespace Application.Core.Login.Shared
{
    /// <summary>
    /// 部分数据在内存里
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class DataStorageBase<TKey, TModel, TEntity> : IDataStorage
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

        protected DataStorageBase(StorageCategory category, IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, ILogger<DataStorageBase<TKey, TModel, TEntity>> logger)
        {
            Category = category;
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _logger = logger;
        }

        public virtual async Task InitializeAsync(DBContext dbContext)
        {
            _logger.LogInformation("正在初始化 {DataCategory}", Category);

            _localId = (await dbContext.Set<TEntity>()
                .IgnoreQueryFilters()
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync()) ?? default!;
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

        public virtual List<TModel> Query(Expression<Func<TEntity, bool>> dbExpression, Func<TModel, bool> localExpression)
        {
            var localData = QueryLocal(localExpression);
            var localIds = localData.Select(x => GetKey(x)).ToList();

            List<TModel> dbList = [];
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbData = dbContext.Set<TEntity>().Where(dbExpression);
            // 用List不要用数组 
            if (localIds.Count > 0)
            {
                dbData = dbData.Where(x => !localIds.Contains(x.Id));
            }

            foreach (var dbItem in dbData)
            {
                var model = MapModel(dbItem);
                SetCache(model);
                dbList.Add(model);
            }

            localData.AddRange(dbList);

            return localData;
        }

        public virtual TModel? Find(Expression<Func<TEntity, bool>> dbExpression, Func<TModel, bool> localExpression)
        {
            var localDataR = _localData.Values.AsValueEnumerable().Where(x => localExpression(x.Data)).FirstOrDefault();
            if (localDataR != null)
            {
                if (localDataR.Flag != StoreFlag.Remove)
                    return localDataR.Data;
                else
                    return null;
            }

            using var dbContext = _dbContextFactory.CreateDbContext();

            var dbData = dbContext.Set<TEntity>().Where(dbExpression).FirstOrDefault();
            if (dbData != null)
            {
                var dbModel = MapModel(dbData);
                SetCache(dbModel);
                return dbModel;
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

            using var dbContext = _dbContextFactory.CreateDbContext();

            var dbData = dbContext.Set<TEntity>().Where(x => x.Id.Equals(key)).FirstOrDefault();
            if (dbData != null)
            {
                var localData = MapModel(dbData);
                SetCache(localData);
                return localData;
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
