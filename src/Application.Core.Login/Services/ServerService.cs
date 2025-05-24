using Application.Core.Login.Datas;
using Application.EF;
using Application.Utility.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Core.Login.Services
{
    internal class ServerService
    {
        readonly ILogger<ServerService> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly AccountManager _accountManager;

        public ServerService(ILogger<ServerService> logger, IDbContextFactory<DBContext> dbContextFactory, AccountManager accountManager)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;

            _accountManager = accountManager;
        }

        public async Task SetupDataBase()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            await InitializeDataBase(dbContext);

            await using var dbTrans = await dbContext.Database.BeginTransactionAsync();
            await SetAllMerchantsInactive(dbContext);
            await CleanNxcodeCoupons(dbContext);

            await _accountManager.SetupAccountPlayerCache(dbContext);

            await dbTrans.CommitAsync();
        }

        private async Task InitializeDataBase(DBContext dbContext)
        {
            _logger.LogInformation("初始化数据库...");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                _logger.LogInformation("数据库迁移...");
                await dbContext.Database.MigrateAsync();
                _logger.LogInformation("数据库迁移成功");

                sw.Stop();
                _logger.LogInformation("初始化数据库成功，耗时{StarupCost}秒", sw.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化数据库失败");
                throw;
            }
        }

        private static async Task SetAllMerchantsInactive(DBContext dbContext)
        {
            await dbContext.Characters.ExecuteUpdateAsync(x => x.SetProperty(y => y.HasMerchant, false));
        }

        private static async Task CleanNxcodeCoupons(DBContext dbContext)
        {
            if (!YamlConfig.config.server.USE_CLEAR_OUTDATED_COUPONS)
            {
                return;
            }

            long timeClear = DateTimeOffset.UtcNow.AddDays(-14).ToUnixTimeMilliseconds();

            var codeList = dbContext.Nxcodes.Where(x => x.Expiration <= timeClear).ToList();
            var codeIdList = codeList.Select(x => x.Id).ToList();
            await dbContext.NxcodeItems.Where(x => codeIdList.Contains(x.Codeid)).ExecuteDeleteAsync();
            dbContext.Nxcodes.RemoveRange(codeList);
            await dbContext.SaveChangesAsync();
        }
    }
}
