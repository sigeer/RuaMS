using Application.Core.Login.Datas;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Services
{
    /// <summary>
    /// 内存数据保存到数据库服务
    /// </summary>
    public class StorageService
    {
        protected System.Threading.Channels.Channel<StorageType> packetChannel;
        readonly CharacterManager _chrManager;
        readonly AccountManager _accountManager;
        readonly ILogger<StorageService> _logger;
        public StorageService(CharacterManager characterManager,  ILogger<StorageService> logger, AccountManager accountManager)
        {
            _chrManager = characterManager;
            _accountManager = accountManager;
            _logger = logger;
            packetChannel = System.Threading.Channels.Channel.CreateUnbounded<StorageType>();
            Task.Run(async () =>
            {
                await foreach (var p in packetChannel.Reader.ReadAllAsync())
                {
                    switch (p)
                    {
                        case StorageType.Full:
                            await _chrManager.CommitAsync();
                            break;
                        case StorageType.AccountOnly:
                            await _accountManager.CommitAsync();
                            break;
                        case StorageType.CharcterOnly:
                            break;
                        case StorageType.InventoryOnly:
                            break;
                        default:
                            break;
                    }

                }
            });
        }

        public bool CommitAll()
        {
            if (!packetChannel.Writer.TryWrite(StorageType.Full))
            {
                _logger.LogCritical("CommitByAuto");
                return false;
            }
            return true;
        }
    }

    public enum StorageType
    {
        /// <summary>
        /// 全部 完整的 CharacterValueObject
        /// </summary>
        Full,
        /// <summary>
        /// 仅更新Account数据 CharacterValueObject.Account
        /// </summary>
        AccountOnly,
        /// <summary>
        /// 仅更新角色数据 CharacterValueObject.Character
        /// </summary>
        CharcterOnly,
        /// <summary>
        /// 仅更新背包数据 CharacterValueObject.Items
        /// </summary>
        InventoryOnly,
    }
}
