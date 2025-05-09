using Application.Core.Game.Relation;
using Application.Core.Game.TheWorld;
using Application.Core.model;
using Application.Shared.Configs;
using Application.Shared.MapObjects;
using Application.Shared.Relations;
using net.packet;

namespace Application.Core.ServerTransports
{
    /// <summary>
    /// 请求MasterServer
    /// </summary>
    public interface IChannelServerTransport : IServerTransport
    {
        public long GetCurrentTime();
        public int GetCurrentTimestamp();
        public DateTimeOffset GetServerupTime();

        Task<int> RegisterServer();
        Task<bool> RemoveServer(IWorldChannel server);

        void DropWorldMessage(int type, string message);
        /// <summary>
        /// 向全服发送数据包
        /// </summary>
        /// <param name="p"></param>
        void BroadcastMessage(Packet p);
        /// <summary>
        /// 全服GM发送数据包
        /// </summary>
        /// <param name="p"></param>
        void BroadcastGMMessage(Packet p);

        #region wedding
        CoupleIdPair? GetRelationshipCouple(int cathedralId);
        void PutMarriageQueued(int weddingId, bool isCathedral, bool isPremium, int groomId, int bridgeId);
        bool IsMarriageQueued(int weddingId);
        CoupleIdPair? GetMarriageQueuedCouple(int weddingId);
        KeyValuePair<bool, HashSet<int>> RemoveMarriageQueued(int marriageId);

        int CreateRelationship(int groomId, int brideId);
        int GetRelationshipId(int playerId);
        void DeleteRelationship(int playerId, int partnerId);
        KeyValuePair<bool, bool>? GetMarriageQueuedLocation(int marriageId);
        bool AddMarriageGuest(int marriageId, int playerId);
        CoupleIdPair? GetWeddingCoupleForGuest(int guestId, bool cathedral);
        #endregion

        #region
        void RemoveGuildQueued(int guildId);
        bool IsGuildQueued(int guildId);
        void PutGuildQueued(int guildId);
        #endregion

        /// <summary>
        /// 更新全局倍率设置
        /// </summary>
        /// <param name="updatePatch"></param>
        void SendWorldConfig(WorldConfigPatch updatePatch);

        void DisconnectPlayers(IEnumerable<int> playerIdList);
        #region Team
        int CreateTeam(int playerId);

        void SendExpelFromParty(int operatorId, int expelCid);
        void SendUpdateTeamGlobalData(int partyId, PartyOperation operation, int targetId, string targetName);
        #endregion

        #region player npc
        void SetPlayerNpcMapPodiumData(int mapId, int podumData);
        int GetPlayerNpcMapPodiumData(int mapId);
        void SetPlayerNpcMapStep(int mapId, int step);
        int GetPlayerNpcMapStep(int mapId);

        void RequestRemovePlayerNpc(int mapId, IEnumerable<int> playerNpcObjectId);
        #endregion

        #region
        void SendTimer(int seconds);
        void RemoveTimer();
        #endregion

        #region PlayerShop
        List<OwlSearchResult> OwlSearch(int itemId);
        PlayerShopDto? SendOwlWarp(int mapId, int ownerId, int searchItem);
        int? FindPlayerShopChannel(int ownerId);

        #endregion

        #region Guild

        #endregion
    }
}
