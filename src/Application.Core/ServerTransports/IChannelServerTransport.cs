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
using Polly;
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

        Task<Config.RegisterServerResult> RegisterServer(List<WorldChannel> channels);
        void HealthCheck(ServerProto.MonitorData data);
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
        TeamProto.TeamDto CreateTeam(int playerId);
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
        SyncProto.PlayerGetterDto? GetPlayerData(string clientSession,int cid);
        bool CheckCharacterName(string name);
        void SendBuffObject(int v, SyncProto.PlayerBuffDto playerBuffSaveDto);
        SyncProto.PlayerBuffDto GetBuffObject(int id);
        /// <summary>
        /// 设置玩家在线
        /// </summary>
        /// <param name="id">玩家id</param>
        /// <param name="channelId">频道号</param>
        void SetPlayerOnlined(int id, int channelId);
        Dto.DropAllDto RequestAllReactorDrops();
        int[] RequestReactorSkillBooks();
        CashProto.SpecialCashItemListDto RequestSpecialCashItems();

        GetMyGiftsResponse LoadPlayerGifts(GetMyGiftsRequest request);
        void ClearGifts(int[] giftIdArray);
        bool SendNormalNoteMessage(int senderId, string toName, string noteMessage);
        Dto.NoteDto? DeleteNoteMessage(int id);
        Dto.ShopDto? GetShop(int id, bool isShopId);
        RankProto.LoadCharacterRankResponse LoadPlayerRanking(int topCount);
        void SendToggleCoupon(int v);
        CreatorProto.CreateCharResponseDto SendNewPlayer(CreatorProto.NewPlayerSaveDto data);
        CreatorProto.CreateCharCheckResponse CreatePlayerCheck(CreatorProto.CreateCharCheckRequest request);
        int[][] GetMostSellerCashItems();
        ItemProto.OwlSearchResponse SendOwlSearch(OwlSearchRequest owlSearchRequest);
        ItemProto.OwlSearchRecordResponse GetOwlSearchedItems();
        TeamProto.UpdateTeamResponse SendUpdateTeam(int teamId, PartyOperation operation, int fromId, int toId);
        void SendTeamChat(string name, string chattext);
        TeamProto.GetTeamResponse GetTeam(int party);

        GuildProto.GetGuildResponse GetGuild(int id);
        GuildProto.GetGuildResponse CreateGuild(string guildName, int playerId, int[] members);
        void SendGuildChat(string name, string text);
        void BroadcastGuildMessage(int guildId, int v, string callout);
        void SendUpdateGuildGP(GuildProto.UpdateGuildGPRequest request);
        void SendUpdateGuildRankTitle(GuildProto.UpdateGuildRankTitleRequest request);
        void SendUpdateGuildNotice(GuildProto.UpdateGuildNoticeRequest request);
        void SendUpdateGuildCapacity(GuildProto.UpdateGuildCapacityRequest request);
        void SendUpdateGuildEmblem(GuildProto.UpdateGuildEmblemRequest request);
        void SendGuildDisband(GuildProto.GuildDisbandRequest request);
        void SendChangePlayerGuildRank(GuildProto.UpdateGuildMemberRankRequest request);
        void SendGuildExpelMember(GuildProto.ExpelFromGuildRequest expelFromGuildRequest);
        void SendPlayerLeaveGuild(GuildProto.LeaveGuildRequest leaveGuildRequest);
        void SendPlayerJoinGuild(GuildProto.JoinGuildRequest joinGuildRequest);


        AllianceProto.GetAllianceResponse GetAlliance(int id);
        void SendAllianceChat(string name, string text);
        AllianceProto.CreateAllianceCheckResponse CreateAllianceCheck(AllianceProto.CreateAllianceCheckRequest request);
        AllianceProto.GetAllianceResponse CreateAlliance(int[] masters, string allianceName);
        void SendGuildJoinAlliance(AllianceProto.GuildJoinAllianceRequest guildJoinAllianceRequest);
        void SendGuildLeaveAlliance(AllianceProto.GuildLeaveAllianceRequest guildLeaveAllianceRequest);
        void SendAllianceExpelGuild(AllianceProto.AllianceExpelGuildRequest allianceExpelGuildRequest);
        void SendChangeAllianceLeader(AllianceProto.AllianceChangeLeaderRequest allianceChangeLeaderRequest);
        void SendChangePlayerAllianceRank(AllianceProto.ChangePlayerAllianceRankRequest changePlayerAllianceRankRequest);
        void SendIncreaseAllianceCapacity(AllianceProto.IncreaseAllianceCapacityRequest increaseAllianceCapacityRequest);
        void SendUpdateAllianceRankTitle(AllianceProto.UpdateAllianceRankTitleRequest request);
        void SendUpdateAllianceNotice(AllianceProto.UpdateAllianceNoticeRequest updateAllianceNoticeRequest);
        void SendAllianceDisband(AllianceProto.DisbandAllianceRequest disbandAllianceRequest);
        #endregion

        void SendPlayerJoinChatRoom(Dto.JoinChatRoomRequest joinChatRoomRequest);
        void SendPlayerLeaveChatRoom(Dto.LeaveChatRoomRequst leaveChatRoomRequst);
        void SendChatRoomMesage(Dto.SendChatRoomMessageRequest sendChatRoomMessageRequest);
        void SendCreateChatRoom(Dto.CreateChatRoomRequest createChatRoomRequest);

        void SendInvitation(InvitationProto.CreateInviteRequest request);
        void AnswerInvitation(InvitationProto.AnswerInviteRequest request);

        void RegisterExpedition(ExpeditionProto.ExpeditionRegistry request);
        ExpeditionProto.ExpeditionCheckResponse CanStartExpedition(ExpeditionProto.ExpeditionCheckRequest expeditionCheckRequest);
        ExpeditionProto.QueryChannelExpedtionResponse GetExpeditionInfo();

        void ReceiveNewYearCard(ReceiveNewYearCardRequest receiveNewYearCardRequest);
        void SendNewYearCard(SendNewYearCardRequest sendNewYearCardRequest);
        void SendDiscardNewYearCard(DiscardNewYearCardRequest discardNewYearCardRequest);

        ConfigProto.SetFlyResponse SendSetFly(ConfigProto.SetFlyRequest setFlyRequest);
        void SendReloadEvents(ReloadEventsRequest reloadEventsRequest);
        ItemProto.CreateTVMessageResponse BroadcastTV(ItemProto.CreateTVMessageRequest request);
        ItemProto.UseItemMegaphoneResponse SendItemMegaphone(ItemProto.UseItemMegaphoneRequest request);
        Dto.DropAllDto RequestDropData();
        BaseProto.QueryMonsterCardDataResponse RequestMonsterCardData();
        GuildProto.QueryRankedGuildsResponse RequestRankedGuilds();
        LifeProto.GetAllPLifeResponse GetAllPLife(LifeProto.GetAllPLifeRequest request);
        LifeProto.GetPLifeByMapIdResponse RequestPLifeByMapId(LifeProto.GetPLifeByMapIdRequest requestPLifeByMapIdRequest);
        void SendCreatePLife(LifeProto.CreatePLifeRequest createPLifeRequest);
        void SendRemovePLife(LifeProto.RemovePLifeRequest removePLifeRequest);
        BuyCashItemResponse SendBuyCashItem(BuyCashItemRequest buyCashItemRequest);

        RemoteHiredMerchantDto LoadPlayerHiredMerchant(GetPlayerHiredMerchantRequest getPlayerShopRequest);
        void SyncPlayerShop(SyncPlayerShopRequest request);
        CommitRetrievedResponse CommitRetrievedFromFredrick(CommitRetrievedRequest commitRetrievedRequest);
        ItemProto.CanHiredMerchantResponse CanHiredMerchant(CanHiredMerchantRequest canHiredMerchantRequest);
        void BatchSyncPlayerShop(BatchSyncPlayerShopRequest request);

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
        void ShutdownMaster(ShutdownMasterRequest shutdownMasterRequest);
        void CompleteChannelShutdown(string serverName);
        void SaveAll(Empty empty);
        ServerStateDto GetServerState();

        ItemProto.GacheponDataDto GetGachaponData();
        NameChangeResponse ReigsterNameChange(NameChangeRequest nameChangeRequest);
        void SyncPlayer(PlayerSaveDto data);
        void BatchSyncPlayer(List<PlayerSaveDto> data);
        #region Buddy
        AddBuddyResponse SendAddBuddyRequest(AddBuddyRequest request);
        AddBuddyResponse SendAddBuddyRequest(AddBuddyByIdRequest request);
        void SendBuddyChat(BuddyChatRequest request);
        void SendBuddyMessage(SendBuddyNoticeMessageDto request);
        DeleteBuddyResponse SendDeleteBuddy(DeleteBuddyRequest deleteBuddyRequest);
        #endregion

        SendWhisperMessageResponse SendWhisper(SendWhisperMessageRequest sendWhisperMessageRequest);
        GetLocationResponse GetLocation(GetLocationRequest getLocationRequest);

        void SendYellowTip(YellowTipRequest yellowTipRequest);

        UseCdkResponse UseCdk(UseCdkRequest useCdkRequest);
        void SendEarnTitleMessage(EarnTitleMessageRequest earnTitleMessageRequest);
        bool GainCharacterSlot(int accountId);
    }
}
