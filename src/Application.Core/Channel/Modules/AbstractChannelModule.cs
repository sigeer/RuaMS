using Application.Core.Channel.Events;
using Microsoft.Extensions.Logging;

namespace Application.Core.Channel.Modules
{
    public abstract class AbstractChannelModule
    {
        protected readonly ILogger<AbstractChannelModule> _logger;
        protected readonly WorldChannelServer _server;
        protected string _moduleName;

        protected AbstractChannelModule(WorldChannelServer server, ILogger<AbstractChannelModule> logger)
        {
            _logger = logger;
            _server = server;

            _moduleName = GetType().Assembly.GetName().Name ?? "unknown";
        }

        public virtual void Initialize()
        {
            _logger.LogInformation("模块 {Name}：初始化", _moduleName);
        }

        public virtual void RegisterTask(ITimerManager timerManager)
        {
            _logger.LogInformation("模块 {Name}：注册定时任务", _moduleName);
        }

        /// <summary>
        /// 停止Channel服务器时调用
        /// </summary>
        /// <returns></returns>
        public virtual Task UninstallAsync()
        {
            _logger.LogInformation("模块 {Name}：卸载", _moduleName);
            return Task.CompletedTask;
        }
        public virtual void OnPlayerLevelUp(SyncProto.PlayerFieldChange arg) { }
        public virtual void OnPlayerChangeJob(SyncProto.PlayerFieldChange arg) { }
        public virtual void OnPlayerServerChanged(SyncProto.PlayerFieldChange arg) 
        {
            if (arg.FromChannel == 0 && arg.Channel > 0)
            {
                OnPlayerLogin(arg);
            }
            //if (arg.FromChannel != 0 && arg.Channel == 0)
            //{
            //    await OnPlayerLogoff(obj);
            //}
            //if (arg.Channel == -1)
            //{
            //    await OnPlayerEnterCashShop(obj);
            //}
        }
        public virtual void OnPlayerLogin(SyncProto.PlayerFieldChange data) { }
        public virtual void OnMonsterReward(MonsterRewardEvent evt) { }
    }
}
