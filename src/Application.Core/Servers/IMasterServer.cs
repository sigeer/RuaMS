using Application.Core.Game.Tasks;
using Application.Core.Gameplay.Wedding;
using Application.Core.Gameplay.WorldEvents;
using Application.Core.ServerTransports;
using Application.Shared.Configs;

namespace Application.Core.Servers
{
    /// <summary>
    /// 兼顾调度+登录（原先的Server+World）
    /// </summary>
    public interface IMasterServer : IServerBase<IMasterServerTransport>
    {
        #region world config
        public int Id { get; }
        public int Flag { get; set; }
        public string Name { get; set; }
        public string WhyAmIRecommended { get; set; }
        public string ServerMessage { get; set; }
        public string EventMessage { get; set; }
        public float ExpRate { get; set; }
        public float DropRate { get; set; }
        public float BossDropRate { get; set; }
        public float MesoRate { get; set; }
        public float QuestRate { get; set; }
        public float TravelRate { get; set; }
        public float FishingRate { get; set; }
        public float MobRate { get; set; }
        void UpdateWorldConfig(WorldConfigPatch updatePatch);
        #endregion
        List<ChannelServerWrapper> ChannelServerList { get; }
        int AddChannel(ChannelServerWrapper channel);
        bool RemoveChannel(string instanceId);
        ChannelServerWrapper GetChannel(int channelId);

        WeddingService WeddingInstance { get; }

        bool IsGuildQueued(int guildId);
        void PutGuildQueued(int guildId);
        void RemoveGuildQueued(int guildId);

        //void resetDisabledServerMessages();
    }
}
