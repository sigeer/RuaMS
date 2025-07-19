using Application.Core.Login.Services;
using Application.Core.model;
using Application.EF;
using Application.Utility.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Diagnostics;

namespace Application.Core.Login.Datas
{
    public class ServerManager
    {
        readonly ILogger<ServerManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly MasterServer _masterServer;


        public ServerManager(ILogger<ServerManager> logger, IDbContextFactory<DBContext> dbContextFactory, MasterServer masterServer)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;

            _masterServer = masterServer;
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
                await _masterServer.GuildManager.Initialize(dbContext);

                await _masterServer.NewYearCardManager.Initialize(dbContext);
                await _masterServer.AccountManager.SetupAccountPlayerCache(dbContext);
                await _masterServer.NoteManager.InitializeAsync(dbContext);
                await _masterServer.GiftManager.Initialize(dbContext);
                await _masterServer.RingManager.Initialize(dbContext);

                foreach (var module in _masterServer.Modules)
                {
                    await module.IntializeDatabaseAsync(dbContext);
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
    }
}
