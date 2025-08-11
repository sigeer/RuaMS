using Application.Core.Channel;
using Application.Shared.Login;
using Application.Shared.Servers;
using Application.Shared.Team;
using BaseProto;
using CashProto;
using Config;
using Dto;
using Google.Protobuf.WellKnownTypes;
using ItemProto;
using MessageProto;
using SyncProto;
using System.Net;
using SystemProto;

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

        void DropWorldMessage(MessageProto.DropMessageRequest request);
        /// <summary>
        /// 向全服发送数据包
        /// </summary>
        /// <param name="p"></param>
        void BroadcastMessage(MessageProto.PacketRequest p);

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
        SearchHiredMerchantChannelResponse FindPlayerShopChannel(SearchHiredMerchantChannelRequest request);
        #endregion

        #region Guild

        #endregion

        #region login
        void SendAccountLogout(int accountId);
        IPEndPoint GetChannelEndPoint(int channel);
        AccountLoginStatus UpdateAccountState(int accId, sbyte state);
        void SetCharacteridInTransition(string v, int cid);
        bool HasCharacteridInTransition(string clientSession);
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
        void CompleteTakeItem(TakeItemSubmit takeItemResolve);
        StoreItemsResponse SaveItems(StoreItemsRequest request);
        LoadItemsFromStoreResponse LoadItemFromStore(LoadItemsFromStoreRequest loadItemsFromStoreRequest);
        void BatchSyncMap(List<MapSyncDto> data);
        SendReportResponse SendReport(SendReportRequest sendReportRequest);
        ToggleMonitorPlayerResponse SetMonitor(ToggleMonitorPlayerRequest toggleMonitorPlayerRequest);
        MonitorDataWrapper LoadMonitor();
        ToggleAutoBanIgnoreResponse SetAutoBanIgnored(ToggleAutoBanIgnoreRequest toggleAutoBanIgnoreRequest);
        AutoBanIgnoredWrapper LoadAutobanIgnoreData();
        BanResponse Ban(BanRequest banRequest);
        UnbanResponse Unban(UnbanRequest unbanRequest);
        SetGmLevelResponse SetGmLevel(SetGmLevelRequest setGmLevelRequest);
        ShowOnlinePlayerResponse GetOnlinedPlayers();
        WrapPlayerByNameResponse WarpPlayerByName(WrapPlayerByNameRequest wrapPlayerByNameRequest);
        SummonPlayerByNameResponse SummonPlayerByName(SummonPlayerByNameRequest summonPlayerByNameRequest);
        DisconnectPlayerByNameResponse DisconnectPlayerByName(DisconnectPlayerByNameRequest disconnectPlayerByNameRequest);
        void DisconnectAll(DisconnectAllRequest disconnectAllRequest);
        GetAllClientInfo GetOnliendClientInfo();
        ItemProto.GacheponDataDto GetGachaponData();
        NameChangeResponse ReigsterNameChange(NameChangeRequest nameChangeRequest);
        void BatchSyncPlayer(List<PlayerSaveDto> data);
        AddBuddyResponse SendAddBuddyRequest(AddBuddyRequest request);
        AddBuddyResponse SendAddBuddyRequest(AddBuddyByIdRequest request);
        void SendBuddyChat(BuddyChatRequest request);
        void SendBuddyMessage(SendBuddyNoticeMessageDto request);
        DeleteBuddyResponse SendDeleteBuddy(DeleteBuddyRequest deleteBuddyRequest);
        SendWhisperMessageResponse SendWhisper(SendWhisperMessageRequest sendWhisperMessageRequest);
        GetLocationResponse GetLocation(GetLocationRequest getLocationRequest);
        void CompleteShutdown(CompleteShutdownRequest completeShutdownRequest);
        void ShutdownMaster(ShutdownMasterRequest shutdownMasterRequest);
        void SaveAll(Empty empty);
        void SendYellowTip(YellowTipRequest yellowTipRequest);
        ExpeditionProto.QueryChannelExpedtionResponse GetExpeditionInfo();
        UseCdkResponse UseCdk(UseCdkRequest useCdkRequest);
        ServerStateDto GetServerStats();
    }
}
