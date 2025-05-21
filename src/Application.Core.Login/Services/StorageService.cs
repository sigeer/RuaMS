using Application.Core.Login.Datas;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Application.Core.Login.Services
{
    /// <summary>
    /// 内存数据保存到数据库服务
    /// </summary>
    public class StorageService
    {
        protected System.Threading.Channels.Channel<StorageType> packetChannel;
        readonly DataStorage _dataStorage;
        readonly AccountManager _accountManager;

        protected readonly ILogger<StorageService> _logger;
        public StorageService(DataStorage chrStorage,  ILogger<StorageService> logger, AccountManager accountManager)
        {
            _dataStorage = chrStorage;
            _accountManager = accountManager;
            _logger = logger;
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
                            await _dataStorage.CommitCharacterAsync();
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

        public async Task CommitAllImmediately()
        {
            await _dataStorage.CommitCharacterAsync();
            await _dataStorage.CommitAccountLoginRecord();
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
