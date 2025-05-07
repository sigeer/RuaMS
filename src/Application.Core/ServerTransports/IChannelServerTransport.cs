using Application.Core.Game.TheWorld;
using Application.Core.model;
using Application.Shared.Configs;

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

        Task<int> RegisterServer(IWorldChannel server);

        void DropWorldMessage(int type, string message);

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
    }
}
