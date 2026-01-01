using Application.Core.Channel;
using Application.Shared.Events;
using Application.Shared.Login;
using Application.Shared.Servers;
using Application.Shared.Team;
using BaseProto;
using CashProto;
using Config;
using CreatorProto;
using Dto;
using DueyDto;
using Google.Protobuf.WellKnownTypes;
using GuildProto;
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

        Task RegisterServer(List<ChannelConfig> channels, CancellationToken cancellationToken = default);
        void HealthCheck(ServerProto.MonitorData data);
        Task DropWorldMessage(MessageProto.DropMessageRequest request);
        /// <summary>
        /// 向全服发送数据包
        /// </summary>
        /// <param name="p"></param>
        Task BroadcastMessage(MessageProto.PacketRequest p);

        #region
        void RemoveGuildQueued(int guildId);
        bool IsGuildQueued(int guildId);
        void PutGuildQueued(int guildId);
        #endregion

        /// <summary>
        /// 更新全局倍率设置
        /// </summary>
        /// <param name="updatePatch"></param>
        Task SendWorldConfig(Config.WorldConfig updatePatch);

        #region Team
        TeamProto.TeamDto CreateTeam(int playerId);
        #endregion

        #region
        Task SendTimer(int seconds);
        Task RemoveTimer();
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
        Task SetPlayerOnlined(int id, int channelId);
        Dto.DropAllDto RequestAllReactorDrops();
        int[] RequestReactorSkillBooks();
        CashProto.SpecialCashItemListDto RequestSpecialCashItems();

        GetMyGiftsResponse LoadPlayerGifts(GetMyGiftsRequest request);
        void ClearGifts(int[] giftIdArray);
        bool SendNormalNoteMessage(int senderId, string toName, string noteMessage);
        Dto.NoteDto? DeleteNoteMessage(int id);
        Dto.ShopDto? GetShop(int id, bool isShopId);
        RankProto.LoadCharacterRankResponse LoadPlayerRanking(int topCount);
        Task SendToggleCoupon(int v);
        CreatorProto.CreateCharResponseDto SendNewPlayer(CreatorProto.NewPlayerSaveDto data);
        CreatorProto.CreateCharCheckResponse CreatePlayerCheck(CreatorProto.CreateCharCheckRequest request);
        int[][] GetMostSellerCashItems();
        ItemProto.OwlSearchResponse SendOwlSearch(OwlSearchRequest owlSearchRequest);
        ItemProto.OwlSearchRecordResponse GetOwlSearchedItems();
        Task SendUpdateTeam(int teamId, PartyOperation operation, int fromId, int toId);
        TeamProto.GetTeamResponse GetTeam(int party);

        GuildProto.GetGuildResponse GetGuild(int id);
        GuildProto.GetGuildResponse CreateGuild(string guildName, int playerId, int[] members);
        void BroadcastGuildMessage(int guildId, int v, string callout);
        Task SendUpdateGuildGP(GuildProto.UpdateGuildGPRequest request);
        Task SendUpdateGuildRankTitle(GuildProto.UpdateGuildRankTitleRequest request);
        Task SendUpdateGuildNotice(GuildProto.UpdateGuildNoticeRequest request);
        Task SendUpdateGuildCapacity(GuildProto.UpdateGuildCapacityRequest request);
        Task SendUpdateGuildEmblem(GuildProto.UpdateGuildEmblemRequest request);
        Task SendGuildDisband(GuildProto.GuildDisbandRequest request);
        Task SendChangePlayerGuildRank(GuildProto.UpdateGuildMemberRankRequest request);
        Task SendGuildExpelMember(GuildProto.ExpelFromGuildRequest expelFromGuildRequest);
        Task SendPlayerLeaveGuild(GuildProto.LeaveGuildRequest leaveGuildRequest);
        Task SendPlayerJoinGuild(GuildProto.JoinGuildRequest joinGuildRequest);


        AllianceProto.GetAllianceResponse GetAlliance(int id);
        AllianceProto.CreateAllianceCheckResponse CreateAllianceCheck(AllianceProto.CreateAllianceCheckRequest request);
        AllianceProto.GetAllianceResponse CreateAlliance(int[] masters, string allianceName);
        Task SendGuildJoinAlliance(AllianceProto.GuildJoinAllianceRequest guildJoinAllianceRequest);
        Task SendGuildLeaveAlliance(AllianceProto.GuildLeaveAllianceRequest guildLeaveAllianceRequest);
        Task SendAllianceExpelGuild(AllianceProto.AllianceExpelGuildRequest allianceExpelGuildRequest);
        Task SendChangeAllianceLeader(AllianceProto.AllianceChangeLeaderRequest allianceChangeLeaderRequest);
        Task SendChangePlayerAllianceRank(AllianceProto.ChangePlayerAllianceRankRequest changePlayerAllianceRankRequest);
        Task SendIncreaseAllianceCapacity(AllianceProto.IncreaseAllianceCapacityRequest increaseAllianceCapacityRequest);
        Task SendUpdateAllianceRankTitle(AllianceProto.UpdateAllianceRankTitleRequest request);
        Task SendUpdateAllianceNotice(AllianceProto.UpdateAllianceNoticeRequest updateAllianceNoticeRequest);
        Task SendAllianceDisband(AllianceProto.DisbandAllianceRequest disbandAllianceRequest);
        #endregion

        Task SendPlayerJoinChatRoom(Dto.JoinChatRoomRequest joinChatRoomRequest);
        Task SendPlayerLeaveChatRoom(Dto.LeaveChatRoomRequst leaveChatRoomRequst);
        Task SendChatRoomMesage(Dto.SendChatRoomMessageRequest sendChatRoomMessageRequest);
        Task SendCreateChatRoom(Dto.CreateChatRoomRequest createChatRoomRequest);

        Task SendInvitation(InvitationProto.CreateInviteRequest request);
        Task AnswerInvitation(InvitationProto.AnswerInviteRequest request);

        void RegisterExpedition(ExpeditionProto.ExpeditionRegistry request);
        ExpeditionProto.ExpeditionCheckResponse CanStartExpedition(ExpeditionProto.ExpeditionCheckRequest expeditionCheckRequest);

        Task ReceiveNewYearCard(ReceiveNewYearCardRequest receiveNewYearCardRequest);
        Task SendNewYearCard(SendNewYearCardRequest sendNewYearCardRequest);
        Task SendDiscardNewYearCard(DiscardNewYearCardRequest discardNewYearCardRequest);

        ConfigProto.SetFlyResponse SendSetFly(ConfigProto.SetFlyRequest setFlyRequest);
        Task SendReloadEvents(ReloadEventsRequest reloadEventsRequest);
        Task BroadcastTV(ItemProto.CreateTVMessageRequest request);
        Task SendItemMegaphone(ItemProto.UseItemMegaphoneRequest request);
        Dto.DropAllDto RequestDropData();
        BaseProto.QueryMonsterCardDataResponse RequestMonsterCardData();
        GuildProto.QueryRankedGuildsResponse RequestRankedGuilds();
        LifeProto.GetAllPLifeResponse GetAllPLife(LifeProto.GetAllPLifeRequest request);
        LifeProto.GetPLifeByMapIdResponse RequestPLifeByMapId(LifeProto.GetPLifeByMapIdRequest requestPLifeByMapIdRequest);
        Task SendCreatePLife(LifeProto.CreatePLifeRequest createPLifeRequest);
        Task SendRemovePLife(LifeProto.RemovePLifeRequest removePLifeRequest);
        BuyCashItemResponse SendBuyCashItem(BuyCashItemRequest buyCashItemRequest);

        RemoteHiredMerchantDto LoadPlayerHiredMerchant(GetPlayerHiredMerchantRequest getPlayerShopRequest);
        void SyncPlayerShop(SyncPlayerShopRequest request);
        CommitRetrievedResponse CommitRetrievedFromFredrick(CommitRetrievedRequest commitRetrievedRequest);
        ItemProto.CanHiredMerchantResponse CanHiredMerchant(CanHiredMerchantRequest canHiredMerchantRequest);
        void BatchSyncPlayerShop(BatchSyncPlayerShopRequest request);

        StoreItemsResponse SaveItems(StoreItemsRequest request);
        LoadItemsFromStoreResponse LoadItemFromStore(LoadItemsFromStoreRequest loadItemsFromStoreRequest);
        Task BatchSyncMap(List<MapSyncDto> data);
        Task SendReport(SendReportRequest sendReportRequest);
        Task SetMonitor(ToggleMonitorPlayerRequest toggleMonitorPlayerRequest);
        MonitorDataWrapper LoadMonitor();
        Task SetAutoBanIgnored(ToggleAutoBanIgnoreRequest toggleAutoBanIgnoreRequest);
        AutoBanIgnoredWrapper LoadAutobanIgnoreData();
        Task Ban(BanRequest banRequest);
        Task Unban(UnbanRequest unbanRequest);
        Task SetGmLevel(SetGmLevelRequest setGmLevelRequest);
        ShowOnlinePlayerResponse GetOnlinedPlayers();
        Task WarpPlayerByName(WrapPlayerByNameRequest wrapPlayerByNameRequest);
        Task SummonPlayerByName(SummonPlayerByNameRequest summonPlayerByNameRequest);
        Task DisconnectPlayerByName(DisconnectPlayerByNameRequest disconnectPlayerByNameRequest);
        GetAllClientInfo GetOnliendClientInfo();
        Task ShutdownMaster(ShutdownMasterRequest shutdownMasterRequest);
        Task CompleteChannelShutdown();
        ServerStateDto GetServerState();

        ItemProto.GacheponDataDto GetGachaponData();
        NameChangeResponse ReigsterNameChange(NameChangeRequest nameChangeRequest);
        Task SyncPlayer(PlayerSaveDto data, SyncCharacterTrigger trigger = SyncCharacterTrigger.Unknown, bool saveDB = false);
        Task BatchSyncPlayer(List<PlayerSaveDto> data, bool saveDB = false);
        #region Buddy
        Task SendAddBuddyRequest(BuddyProto.AddBuddyRequest request);
        Task SendAddBuddyRequest(BuddyProto.AddBuddyByIdRequest request);
        Task SendBuddyMessage(BuddyProto.SendBuddyNoticeMessageDto request);
        Task SendDeleteBuddy(BuddyProto.DeleteBuddyRequest deleteBuddyRequest);

        Task GetLocation(BuddyProto.GetLocationRequest getLocationRequest);
        #endregion

        Task SendWhisper(SendWhisperMessageRequest sendWhisperMessageRequest);
        

        UseCdkResponse UseCdk(UseCdkRequest useCdkRequest);
        bool GainCharacterSlot(int accountId);
        void SendGuildPacket(GuildPacketRequest guildPacketRequest);
        Task SendMultiChatAsync(int type, string fromName, string msg, int[] receivers);
        Task SaveAllNotifyAsync();
        Task DisconnectAllNotifyAsync();
        Task CreatePlayerResponseAsync(CreateCharResponseDto res, CancellationToken cancellationToken);

        #region Duey
        Task CreateDueyPackage(CreatePackageRequest request);
        Task TakeDueyPackage(TakeDueyPackageRequest request);
        Task RequestRemovePackage(RemovePackageRequest request);
        Task GetDueyPackagesByPlayerId(GetPlayerDueyPackageRequest request);
        Task TakeDueyPackageCommit(TakeDueyPackageCommit takeDueyPackageCommit);
        #endregion
    }
}
