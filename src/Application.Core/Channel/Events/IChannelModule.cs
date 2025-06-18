using Dto;

namespace Application.Core.Channel.Events
{
    public interface IChannelModule
    {
        void Initialize();
        public void OnPlayerLevelUp(Dto.PlayerLevelJobChange arg);
        public void OnPlayerChangeJob(Dto.PlayerLevelJobChange arg);
        public void OnPlayerLogin(Dto.PlayerOnlineChange data);
        public void OnMonsterReward(MonsterRewardEvent evt);
    }
}
