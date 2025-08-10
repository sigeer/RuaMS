using Application.Core.Game.Controllers;
using Application.Core.Login.Services;
using Application.Core.Login.Shared;
using Application.EF;
using Application.Utility.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Core.Login.Datas
{
    public class ServerManager: TimelyControllerBase
    {
        readonly ILogger<ServerManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly MasterServer _masterServer;
        protected System.Threading.Channels.Channel<bool> packetChannel;
        readonly DataStorage _dataStorage;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public ServerManager(ILogger<ServerManager> logger, IDbContextFactory<DBContext> dbContextFactory, MasterServer masterServer, DataStorage chrStorage)
            :base("DataStorage", TimeSpan.FromHours(1), TimeSpan.FromHours(1))
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;

            _masterServer = masterServer;

            _dataStorage = chrStorage;
            _logger = logger;
            packetChannel = System.Threading.Channels.Channel.CreateUnbounded<bool>();
            // 定时触发、特殊事件触发、关闭服务器触发
            Task.Run(async () =>
            {
                await foreach (var p in packetChannel.Reader.ReadAllAsync())
                {
                    await CommitAllImmediately();

                }
            });
        }

        public async Task Setup()
        {
            await SetupDataBase();

            _masterServer.ServiceProvider.GetRequiredService<InvitationService>().Initialize();
        }

        private async Task SetupDataBase()
        {
            _logger.LogInformation("初始化数据库...");
            try
            {
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                await InitializeDataBase(dbContext);

                await using var dbTrans = await dbContext.Database.BeginTransactionAsync();
                await CleanNxcodeCoupons(dbContext);
                await _masterServer.CouponManager.Initialize(dbContext);

                await _masterServer.AccountManager.SetupAccountPlayerCache(dbContext);

                foreach (var item in _masterServer.ServiceProvider.GetServices<IStorage>())
                {
                    await item.InitializeAsync(dbContext);
                }
                await dbTrans.CommitAsync();
                _logger.LogInformation("初始化数据库>>>成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化数据库>>>失败");
                throw;
            }
        }

        private async Task InitializeDataBase(DBContext dbContext)
        {
            try
            {
                _logger.LogInformation("数据库迁移...");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                await dbContext.Database.MigrateAsync();
                sw.Stop();
                _logger.LogInformation("数据库迁移>>>成功，耗时{StarupCost}秒", sw.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据库迁移>>>失败");
                throw;
            }
        }

        private async Task CleanNxcodeCoupons(DBContext dbContext)
        {
            if (!YamlConfig.config.server.USE_CLEAR_OUTDATED_COUPONS)
            {
                return;
            }

            long timeClear = _masterServer.getCurrentTime() - (long)TimeSpan.FromDays(14).TotalMilliseconds;

            var codeList = dbContext.CdkCodes.Where(x => x.Expiration <= timeClear).ToList();
            var codeIdList = codeList.Select(x => x.Id).ToList();
            await dbContext.CdkItems.Where(x => codeIdList.Contains(x.CodeId)).ExecuteDeleteAsync();
            dbContext.CdkCodes.RemoveRange(codeList);
            await dbContext.SaveChangesAsync();
        }

        // 后期会尽量避免直接存放玩家名称，改名直接修改character.name
        //private void ApplyAllNameChanges(DBContext dbContext)
        //{
        //    try
        //    {
        //        List<NameChangePair> changedNames = new();
        //        using var dbTrans = dbContext.Database.BeginTransaction();
        //        var allChanges = dbContext.Namechanges.Where(x => x.CompletionTime == null).ToList();
        //        allChanges.ForEach(x =>
        //        {
        //            bool success = CharacterManager.doNameChange(dbContext, x.Characterid, x.Old, x.New, x.Id);
        //            if (!success)
        //                dbTrans.Rollback();
        //            else
        //            {
        //                dbTrans.Commit();
        //                changedNames.Add(new(x.Old, x.New));
        //            }
        //        });

        //        //log
        //        foreach (var namePair in changedNames)
        //        {
        //            _logger.LogInformation("Name change applied - from: \"{CharacterName}\" to \"{CharacterName}\"", namePair.OldName, namePair.NewName);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogWarning(e, "Failed to retrieve list of pending name changes");
        //        throw;
        //    }
        //}

        #region Write

        protected override void HandleRun()
        {
            CommitAll();
        }
        public bool CommitAll()
        {
            return packetChannel.Writer.TryWrite(true);
        }

        public async Task CommitAllImmediately()
        {
            if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(20)))
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

                foreach (var item in _masterServer.ServiceProvider.GetServices<IStorage>())
                {
                    await item.Commit(dbContext);
                }

                await dbTrans.CommitAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }
        #endregion
    }
}
