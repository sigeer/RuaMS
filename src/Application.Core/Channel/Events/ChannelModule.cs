using Microsoft.Extensions.Logging;

namespace Application.Core.Channel.Events
{
    public abstract class ChannelModule
    {
        protected readonly ILogger<ChannelModule> _logger;
        protected readonly WorldChannelServer _server;

        protected ChannelModule(WorldChannelServer server, ILogger<ChannelModule> logger)
        {
            _logger = logger;
            _server = server;
        }

        public virtual void Initialize()
        {
            _logger.LogInformation($"模块 {GetType().Assembly.FullName} 加载完成");
        }
        public virtual void OnPlayerLevelUp(Dto.PlayerLevelJobChange arg) { }
        public virtual void OnPlayerChangeJob(Dto.PlayerLevelJobChange arg) { }
        public virtual void OnPlayerLogin(Dto.PlayerOnlineChange data) { }
        public virtual void OnMonsterReward(MonsterRewardEvent evt) { }
    }
}
