using Application.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Services
{
    /// <summary>
    /// 内存数据保存到数据库服务
    /// </summary>
    public class StorageService
    {
        protected System.Threading.Channels.Channel<StorageType> packetChannel;
        readonly DataStorage _dataStorage;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        protected readonly ILogger<StorageService> _logger;
        public StorageService(DataStorage chrStorage, ILogger<StorageService> logger, IDbContextFactory<DBContext> dbContextFactory)
        {
            _dataStorage = chrStorage;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            packetChannel = System.Threading.Channels.Channel.CreateUnbounded<StorageType>();
            // 定时触发、特殊事件触发、关闭服务器触发
            Task.Run(async () =>
            {
                await foreach (var p in packetChannel.Reader.ReadAllAsync())
                {
                    switch (p)
                    {
                        case StorageType.All:
                            await CommitAllImmediately();
                            break;
                        case StorageType.CharcterOnly:
                            await CommitCharacter();
                            break;
                        case StorageType.MerchantOnly:
                            break;
                        default:
                            break;
                    }

                }
            });
        }

        private bool CommitType(StorageType type)
        {
            if (!packetChannel.Writer.TryWrite(type))
            {
                _logger.LogCritical("CommitType {Type}", type);
                return false;
            }
            return true;
        }
        public bool CommitAll()
        {
            return CommitType(StorageType.All);
        }

        public async Task CommitCharacter()
        {
            if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(5)))
            {
                _logger.LogInformation("失败：已经有一个保存操作正在进行中");
                return;
            }

            try
            {
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                await using var dbTrans = await dbContext.Database.BeginTransactionAsync();

                await _dataStorage.CommitCharacterAsync(dbContext);
                await _dataStorage.CommitAccountCtrlAsync(dbContext);
                await _dataStorage.CommitAccountGameAsync(dbContext);
                await _dataStorage.CommitAccountLoginRecord(dbContext);

                await dbTrans.CommitAsync();
            }
            finally
            {
                _semaphore.Release();
            }

        }

        public async Task CommitAllImmediately()
        {
            if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(5)))
            {
                _logger.LogInformation("失败：已经有一个保存操作正在进行中");
                return;
            }

            try
            {
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                await using var dbTrans = await dbContext.Database.BeginTransactionAsync();

                await _dataStorage.CommitCharacterAsync(dbContext);
                await _dataStorage.CommitAccountCtrlAsync(dbContext);
                await _dataStorage.CommitAccountGameAsync(dbContext);
                await _dataStorage.CommitAccountLoginRecord(dbContext);

                await dbTrans.CommitAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public enum StorageType
    {
        /// <summary>
        /// 全部，其他所有项
        /// </summary>
        All,
        /// <summary>
        /// 仅更新角色及相关数据
        /// </summary>
        CharcterOnly,
        /// <summary>
        /// 更新玩家商店道具、雇佣商人道具
        /// </summary>
        MerchantOnly,
    }
}
