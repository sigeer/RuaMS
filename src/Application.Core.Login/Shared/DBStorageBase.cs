using Application.Core.EF;
using Application.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Shared
{
    public abstract class DBStorageBase : IDataStorage
    {
        protected readonly IDbContextFactory<DBContext> _dbContextFactory;
        protected readonly IMapper _mapper;
        protected readonly ILogger<DBStorageBase> _logger;

        protected DBStorageBase(StorageCategory category, IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, ILogger<DBStorageBase> logger)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _logger = logger;
            Category = category;
        }

        public StorageCategory Category { get; }

        public virtual Task Commit(DBContext dbContext)
        {
            _logger.LogInformation("正在保存 {DataCategory}...", Category);
            return Task.CompletedTask;
        }
        public virtual Task InitializeAsync(DBContext dbContext)
        {
            _logger.LogInformation("正在初始化 {DataCategory}", Category);
            return Task.CompletedTask;
        }
    }
}
