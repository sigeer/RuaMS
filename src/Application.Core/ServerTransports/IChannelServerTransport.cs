using Application.Core.Datas;
using Application.Core.Game.Life;
using Application.Core.Game.Relation;
using Application.Core.Game.TheWorld;
using Application.Core.model;
using Application.Shared.Configs;
using Application.Shared.Dto;
using Application.Shared.Items;
using Application.Shared.Login;
using Application.Shared.MapObjects;
using Application.Shared.Servers;
using System.Net;

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
        ITeam CreateTeam(int playerId);
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

        #region login
        void SendAccountLogout(int accountId);
        IPEndPoint GetChannelEndPoint(int channel);
        void NotifyPartner(int id);
        AccountLoginStatus UpdateAccountState(int accId, sbyte state);
        void SetCharacteridInTransition(string v, int cid);
        bool HasCharacteridInTransition(string clientSession);
        bool WarpPlayer(string name, int? channel, int mapId, int? portal);
        string LoadExpeditionInfo();
        void ChangePlayerAllianceRank(int targetCharacterId, bool isRaise);
        CharacterValueObject? GetPlayerData(string clientSession, int cid);
        int GetAccountCharacterCount(int accId);
        bool CheckCharacterName(string name);
        void UpdateAccountChracterByAdd(int accountId, int id);
        void SendPlayerObject(PlayerSaveDto characterValueObject);
        void SendRemovePlayerIncomingInvites(int id);
        void SendBuffObject(int v, PlayerBuffSaveDto playerBuffSaveDto);
        PlayerBuffSaveDto GetBuffObject(int id);
        /// <summary>
        /// 设置玩家在线
        /// </summary>
        /// <param name="id">玩家id</param>
        /// <param name="v">频道号</param>
        void SetPlayerOnlined(int id, int v);
        void CallSaveDB();
        Dictionary<int, List<DropDto>> RequestAllReactorDrops();
        int[] RequestReactorSkillBooks();
        SpecialCashItem[] RequestSpecialCashItems();
        void SendGift(int recipient, string from, string message, int sn, int ringid);
        GiftDto[] LoadPlayerGifts(int playerId);
        void ClearGifts(int[] giftIdArray);

        #endregion
    }
}
