using Application.Core.Login.Models;
using Application.EF;
using Application.EF.Entities;
using Application.Utility.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public virtual void OnPlayerLoad(DBContext dbContext, CharacterModel chrModel)
        {

        }

        public virtual void OnPlayerLogin(CharacterLiveObject obj)
        {

        }

        public virtual void OnPlayerLogoff(CharacterLiveObject obj)
        {

        }

        public virtual void OnPlayerMapChanged(CharacterLiveObject character)
        {

        }

        public virtual void OnPlayerEnterCashShop(CharacterLiveObject character)
        {

        }

        public virtual int DeleteCharacterCheck(int id)
        {
            return 0;
        }
        public virtual void OnPlayerDeleted(int id)
        {

        }
    }
}
