using Application.Core.Login.Models;
using Application.EF;
using Application.Utility.Tasks;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Events
{
    public abstract class MasterModule
    {
        protected readonly MasterServer _server;
        protected readonly ILogger<MasterModule> _logger;
        protected string _moduleName;
        protected MasterModule(MasterServer server, ILogger<MasterModule> logger)
        {
            _server = server;
            _logger = logger;

            _moduleName = GetType().Assembly.GetName().Name ?? "unknown";
        }

        /// <summary>
        /// Master服务器初始化数据库调用（在InitializeAsync之前）
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public virtual Task IntializeDatabaseAsync(DBContext dbContext)
        {
            _logger.LogInformation("模块 {Name}：加载数据", _moduleName);
            return Task.CompletedTask;
        }
        /// <summary>
        /// 启动Master服务器时调用
        /// </summary>
        /// <returns></returns>
        public virtual Task InitializeAsync()
        {
            _logger.LogInformation("模块 {Name}：初始化", _moduleName);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Master服务器注册定时任务时调用
        /// </summary>
        public virtual void RegisterTask(ITimerManager timerManager)
        {
            _logger.LogInformation("模块 {Name}：注册定时任务", _moduleName);
        }


        /// <summary>
        /// 停止Master服务器时调用
        /// </summary>
        /// <returns></returns>
        public virtual Task UninstallAsync()
        {
            _logger.LogInformation("模块 {Name}：卸载", _moduleName);
            return Task.CompletedTask;
        }

        public virtual void OnPlayerLogin(CharacterLiveObject obj)
        {

        }
        public virtual int DeleteCharacterCheck(int id)
        {
            return 0;
        }
        public virtual void OnPlayerDeleted(int id)
        {

        }
        /// <summary>
        /// Master服务器统一保存数据时调用
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public virtual Task SaveChangesAsync(DBContext dbContext)
        {
            _logger.LogInformation("模块 {Name}：保存数据库", _moduleName);
            return Task.CompletedTask;
        }
    }
}
