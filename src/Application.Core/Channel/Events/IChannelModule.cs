namespace Application.Core.Channel.Events
{
    public interface IChannelModule
    {
        public void OnPlayerLevelUp(Dto.PlayerLevelJobChange arg);
        public void OnPlayerChangeJob(Dto.PlayerLevelJobChange arg);
        public void OnPlayerLogin(Dto.PlayerOnlineChange data);
        public void OnMonsterReward(MonsterRewardEvent evt);
    }
}
