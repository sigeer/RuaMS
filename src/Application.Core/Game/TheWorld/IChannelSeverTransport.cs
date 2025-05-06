using Application.Core.model;
using Application.Shared.Servers;

namespace Application.Core.Game.TheWorld
{
    /// <summary>
    /// 频道服务器请求调度服务器
    /// </summary>
    public interface IChannelSeverTransport: IServerTransport
    {
        /// <summary>
        /// 向调度服务器注册当前服务器
        /// </summary>
        Task<int> RegisterChannel(IWorldChannel worldChannel);
        Task UnRegisterChannel(string id);

        #region wedding
        CoupleIdPair? GetRelationshipCouple(int cathedralId);
        void PutMarriageQueued(int weddingId, bool isCathedral, bool isPremium, int groomId, int bridgeId);
        bool IsMarriageQueued(int weddingId);
        CoupleIdPair? GetMarriageQueuedCouple(int weddingId);
        KeyValuePair<bool, HashSet<int>> RemoveMarriageQueued(int marriageId);
        #endregion

        float GetWorldMobRate();
        int GetTransportationTime(double travelTime);

        void DisconnectPlayers(IEnumerable<int> playerIdList);

        #region
        void RemoveGuildQueued(int guildId);
        bool IsGuildQueued(int guildId);
        void PutGuildQueued(int guildId);
        #endregion

        void ResetDisabledServerMessages();

        int GetPlayerNpcMapStep(int mapid);
        void SetPlayerNpcMapStep(int mapid, int step);
        int GetPlayerNpcMapPodiumData(int mapid);
        void SetPlayerNpcMapPodiumData(int mapid, int podium);


        void ResetFamilyDailyReps();
    }
}
