using Application.EF;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Shared
{
    public abstract class ReadonlyStorageBase : IDataStorage
    {
        protected ILogger<ReadonlyStorageBase> _logger;

        protected ReadonlyStorageBase(ILogger<ReadonlyStorageBase> logger, StorageCategory category)
        {
            _logger = logger;
            Category = category;
        }

        public StorageCategory Category { get; }
        public virtual Task Commit(DBContext dbContext)
        {
            return Task.CompletedTask;
        }

        public virtual Task InitializeAsync(DBContext dbContext)
        {
            _logger.LogInformation("正在初始化 {DataCategory}", Category);
            return Task.CompletedTask;
        }
    }
}
