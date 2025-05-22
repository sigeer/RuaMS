using Application.Core.Game.GlobalControllers;
using Application.Core.Gameplay.Wedding;
using Application.Core.ServerTransports;
using Application.Shared.Characters;
using Application.Shared.Configs;
using Application.Shared.Dto;
using Application.Shared.Login;
using Application.Shared.Servers;
using net.server;
using System.Net;

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

        InvitationController InvitationController { get; }

        IServiceProvider ServiceProvider { get; }
        List<ChannelServerWrapper> ChannelServerList { get; }
        int AddChannel(ChannelServerWrapper channel);
        bool RemoveChannel(string instanceId);
        ChannelServerWrapper GetChannel(int channelId);
        IPEndPoint GetChannelIPEndPoint(int channelId);

        WeddingService WeddingInstance { get; }

        bool IsGuildQueued(int guildId);
        void PutGuildQueued(int guildId);
        void RemoveGuildQueued(int guildId);

        //void resetDisabledServerMessages();
        int GetWorldCapacityStatus();
        bool IsWorldCapacityFull();

        void RegisterLoginState(ILoginClient c);
        void UnregisterLoginState(ILoginClient c);
        bool ValidateCharacteridInTransition(string clientSession, int charId);
        bool WarpPlayer(string name, int? channel, int mapId, int? portal);
        AccountDto? GetAccountDto(int accId);
        int GetAccountCharacterCount(int accId);
        List<IPlayer> LoadAccountCharactersView(int id);
        int GetAccountIdByAccountName(string name);
        void CommitAccountEntity(AccountDto accountEntity);
        AccountLoginStatus GetAccountLoginStatus(int accId);
        void SaveBuff(int v, PlayerBuffSaveDto playerBuffSaveDto);
        PlayerBuffSaveDto GetBuff(int v);
    }
}
