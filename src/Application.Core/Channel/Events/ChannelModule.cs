using Microsoft.Extensions.Logging;

namespace Application.Core.Channel.Events
{
    public abstract class ChannelModule
    {
        protected readonly ILogger<ChannelModule> _logger;
        protected readonly WorldChannelServer _server;
        protected string _moduleName;

        protected ChannelModule(WorldChannelServer server, ILogger<ChannelModule> logger)
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
        public virtual void OnPlayerLevelUp(Dto.PlayerLevelJobChange arg) { }
        public virtual void OnPlayerChangeJob(Dto.PlayerLevelJobChange arg) { }
        public virtual void OnPlayerLogin(Dto.PlayerOnlineChange data) { }
        public virtual void OnMonsterReward(MonsterRewardEvent evt) { }
    }
}
