using Application.Core.Channel;
using Application.Shared.Login;
using Application.Shared.Servers;
using Application.Shared.Team;
using BaseProto;
using CashProto;
using Dto;
using ItemProto;
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

        Task<Config.RegisterServerResult> RegisterServer(WorldChannelServer server, List<WorldChannel> channels);
        Task<bool> RemoveServer(WorldChannel server);

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
        void SendWorldConfig(Config.WorldConfig updatePatch);

        #region Team
        Dto.TeamDto CreateTeam(int playerId);
        #endregion

        #region
        void SendTimer(int seconds);
        void RemoveTimer();
        #endregion

        #region PlayerShop
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
        Dto.PlayerGetterDto? GetPlayerData(string clientSession, int channelId, int cid);
        int GetAccountCharacterCount(int accId);
        bool CheckCharacterName(string name);
        void UpdateAccountChracterByAdd(int accountId, int id);
        void SendPlayerObject(Dto.PlayerSaveDto characterValueObject);
        void SendRemovePlayerIncomingInvites(int id);
        void SendBuffObject(int v, Dto.PlayerBuffSaveDto playerBuffSaveDto);
        Dto.PlayerBuffSaveDto GetBuffObject(int id);
        /// <summary>
        /// 设置玩家在线
        /// </summary>
        /// <param name="id">玩家id</param>
        /// <param name="channelId">频道号</param>
        void SetPlayerOnlined(int id, int channelId);
        void CallSaveDB();
        Dto.DropAllDto RequestAllReactorDrops();
        int[] RequestReactorSkillBooks();
        Dto.SpecialCashItemListDto RequestSpecialCashItems();

        GetMyGiftsResponse LoadPlayerGifts(GetMyGiftsRequest request);
        void ClearGifts(int[] giftIdArray);
        bool SendNormalNoteMessage(int senderId, string toName, string noteMessage);
        Dto.NoteDto? DeleteNoteMessage(int id);
        Dto.ShopDto? GetShop(int id, bool isShopId);
        int[] GetCardTierSize();
        void SendUnbanAccount(string playerName);
        void AddReport(int v1, int v2, int v3, string description, string v4);
        Rank.RankCharacterList LoadPlayerRanking(int topCount);
        void SendToggleCoupon(int v);
        void UpdateAccount(AccountCtrl accountEntity);
        Dto.CreateCharResponseDto SendNewPlayer(Dto.NewPlayerSaveDto data);
        Dto.CreateCharCheckResponse CreatePlayerCheck(Dto.CreateCharCheckRequest request);
        int[][] GetMostSellerCashItems();
        ItemProto.OwlSearchRecordResponse GetOwlSearchedItems();
        Dto.UpdateTeamResponse SendUpdateTeam(int teamId, PartyOperation operation, int fromId, int toId);
        void SendTeamChat(string name, string chattext);
        Dto.GetTeamResponse GetTeam(int party);
        Dto.GetGuildResponse GetGuild(int id);
        Dto.GetGuildResponse CreateGuild(string guildName, int playerId, int[] members);
        Dto.CreateAllianceCheckResponse CreateAllianceCheck(Dto.CreateAllianceCheckRequest request);
        Dto.GetAllianceResponse CreateAlliance(int[] masters, string allianceName);
        Dto.GetAllianceResponse GetAlliance(int id);
        void SendGuildChat(string name, string text);
        void SendAllianceChat(string name, string text);
        void BroadcastGuildMessage(int guildId, int v, string callout);
        void SendUpdateGuildGP(Dto.UpdateGuildGPRequest request);
        void SendUpdateGuildRankTitle(Dto.UpdateGuildRankTitleRequest request);
        void SendUpdateGuildNotice(Dto.UpdateGuildNoticeRequest request);
        void SendUpdateGuildCapacity(Dto.UpdateGuildCapacityRequest request);
        void SendUpdateGuildEmblem(Dto.UpdateGuildEmblemRequest request);
        void SendGuildDisband(Dto.GuildDisbandRequest request);
        void SendChangePlayerGuildRank(Dto.UpdateGuildMemberRankRequest request);
        void SendGuildExpelMember(Dto.ExpelFromGuildRequest expelFromGuildRequest);
        void SendPlayerLeaveGuild(Dto.LeaveGuildRequest leaveGuildRequest);
        void SendPlayerJoinGuild(Dto.JoinGuildRequest joinGuildRequest);


        void SendGuildJoinAlliance(Dto.GuildJoinAllianceRequest guildJoinAllianceRequest);
        void SendGuildLeaveAlliance(Dto.GuildLeaveAllianceRequest guildLeaveAllianceRequest);
        void SendAllianceExpelGuild(Dto.AllianceExpelGuildRequest allianceExpelGuildRequest);
        void SendChangeAllianceLeader(Dto.AllianceChangeLeaderRequest allianceChangeLeaderRequest);
        void SendChangePlayerAllianceRank(Dto.ChangePlayerAllianceRankRequest changePlayerAllianceRankRequest);
        void SendIncreaseAllianceCapacity(Dto.IncreaseAllianceCapacityRequest increaseAllianceCapacityRequest);
        void SendUpdateAllianceRankTitle(Dto.UpdateAllianceRankTitleRequest request);
        void SendUpdateAllianceNotice(Dto.UpdateAllianceNoticeRequest updateAllianceNoticeRequest);
        void SendAllianceDisband(Dto.DisbandAllianceRequest disbandAllianceRequest);
        #endregion

        void SendPlayerJoinChatRoom(Dto.JoinChatRoomRequest joinChatRoomRequest);
        void SendPlayerLeaveChatRoom(Dto.LeaveChatRoomRequst leaveChatRoomRequst);
        void SendChatRoomMesage(Dto.SendChatRoomMessageRequest sendChatRoomMessageRequest);
        void SendCreateChatRoom(Dto.CreateChatRoomRequest createChatRoomRequest);

        void SendInvitation(Dto.CreateInviteRequest request);
        void AnswerInvitation(Dto.AnswerInviteRequest request);
        void RegisterExpedition(Dto.ExpeditionRegistry request);
        Dto.ExpeditionCheckResponse CanStartExpedition(Dto.ExpeditionCheckRequest expeditionCheckRequest);

        void ReceiveNewYearCard(ReceiveNewYearCardRequest receiveNewYearCardRequest);
        void SendNewYearCard(SendNewYearCardRequest sendNewYearCardRequest);
        void SendDiscardNewYearCard(DiscardNewYearCardRequest discardNewYearCardRequest);
        void SendSetFly(SetFlyRequest setFlyRequest);
        void SendReloadEvents(ReloadEventsRequest reloadEventsRequest);
        void BroadcastMessage(SendTextMessage data);
        void BroadcastTV(ItemProto.CreateTVMessageRequest request);
        void SendItemMegaphone(ItemProto.UseItemMegaphoneRequest request);
        void FinishTransaction(ItemProto.FinishTransactionRequest finishTransactionRequest);
        Dto.DropAllDto RequestDropData();
        Dto.QueryDropperByItemResponse RequestWhoDrops(QueryDropperByItemRequest queryDropperByItemRequest);
        BaseProto.QueryMonsterCardDataResponse RequestMonsterCardData();
        QueryRankedGuildsResponse RequestRankedGuilds();
        GetPLifeByMapIdResponse RequestPLifeByMapId(GetPLifeByMapIdRequest requestPLifeByMapIdRequest);
        void SendCreatePLife(CreatePLifeRequest createPLifeRequest);
        void SendRemovePLife(RemovePLifeRequest removePLifeRequest);
        void SendBuyCashItem(BuyCashItemRequest buyCashItemRequest);
        RemoteHiredMerchantDto LoadPlayerHiredMerchant(GetPlayerHiredMerchantRequest getPlayerShopRequest);
        void SyncPlayerShop(SyncPlayerShopRequest request);
        CommitRetrievedResponse CommitRetrievedFromFredrick(CommitRetrievedRequest commitRetrievedRequest);
        ItemProto.CanHiredMerchantResponse CanHiredMerchant(CanHiredMerchantRequest canHiredMerchantRequest);
        void BatchSyncPlayerShop(BatchSyncPlayerShopRequest request);
        ItemProto.OwlSearchResponse SendOwlSearch(OwlSearchRequest owlSearchRequest);
    }
}
