using Application.Shared.Events;
using Application.Shared.Login;
using Application.Shared.Servers;
using Application.Shared.Team;
using Google.Protobuf;
using System.Net;

namespace Application.Core.ServerTransports
{
    /// <summary>
    /// 请求MasterServer
    /// </summary>
    public interface IChannelServerTransport : IServerTransport
    {
        Task SendAsync(int type, CancellationToken cancellationToken = default);
        Task SendAsync(int type, IMessage message, CancellationToken cancellationToken = default);
        public long GetCurrentTime();
        public int GetCurrentTimestamp();

        Task RegisterServer(List<ChannelConfig> channels, CancellationToken cancellationToken = default);
        void HealthCheck(ProtoModel.MonitorData data);
        Task DropWorldMessage(ProtoService.DropMessageRequest request);
        /// <summary>
        /// 向全服发送数据包
        /// </summary>
        /// <param name="p"></param>
        Task BroadcastMessage(ProtoService.PacketRequest p);

        #region
        void RemoveGuildQueued(int guildId);
        bool IsGuildQueued(int guildId);
        void PutGuildQueued(int guildId);
        #endregion

        /// <summary>
        /// 更新全局倍率设置
        /// </summary>
        /// <param name="updatePatch"></param>
        Task SendWorldConfig(ProtoModel.WorldConfig updatePatch);

        #region Team
        Task CreateTeam(ProtoService.CreateTeamRequest request);
        #endregion

        #region
        Task SendTimer(int seconds);
        Task RemoveTimer();
        #endregion



        #region Guild

        #endregion

        #region login
        void SendAccountLogout(int accountId);
        IPEndPoint GetChannelEndPoint(int channel);
        AccountLoginStatus UpdateAccountState(int accId, sbyte state);
        void SetCharacteridInTransition(string v, int cid);
        bool HasCharacteridInTransition(string clientSession);
        ProtoModel.PlayerGetterProto? GetPlayerData(string clientSession, int cid);
        bool CheckCharacterName(string name);
        void SendBuffObject(int v, ProtoModel.PlayerBuffProto playerBuffSaveDto);
        ProtoModel.PlayerBuffProto GetBuffObject(int id);
        /// <summary>
        /// 设置玩家在线
        /// </summary>
        /// <param name="id">玩家id</param>
        /// <param name="channelId">频道号</param>
        Task SetPlayerOnlined(int id, int channelId);
        ProtoModel.DropAllProto RequestAllReactorDrops();
        int[] RequestReactorSkillBooks();
        ProtoModel.SpecialCashItemListProto RequestSpecialCashItems();

        ProtoService.GetMyGiftsResponse LoadPlayerGifts(ProtoService.GetMyGiftsRequest request);
        void ClearGifts(int[] giftIdArray);
        Task<bool> SendNormalNoteMessage(int senderId, string toName, string noteMessage);
        ProtoModel.NoteProto? DeleteNoteMessage(int id);
        ProtoModel.ShopProto? GetShop(int id, bool isShopId);
        ProtoService.LoadCharacterRankResponse LoadPlayerRanking(int topCount);
        int[][] GetMostSellerCashItems();
        ProtoService.OwlSearchResponse SendOwlSearch(ProtoService.OwlSearchRequest owlSearchRequest);
        ProtoService.OwlSearchRecordResponse GetOwlSearchedItems();
        Task SendUpdateTeam(int teamId, PartyOperation operation, int fromId, int toId, int reason);
        ProtoService.GetTeamResponse GetTeam(int party);

        ProtoService.GetGuildResponse GetGuild(int id);
        Task CreateGuild(ProtoService.CreateGuildRequest request);
        Task BroadcastGuildMessage(int guildId, int v, string callout);
        Task SendUpdateGuildGP(ProtoService.UpdateGuildGPRequest request);
        Task SendUpdateGuildRankTitle(ProtoService.UpdateGuildRankTitleRequest request);
        Task SendUpdateGuildNotice(ProtoService.UpdateGuildNoticeRequest request);
        Task SendUpdateGuildCapacity(ProtoService.UpdateGuildCapacityRequest request);
        Task SendUpdateGuildEmblem(ProtoService.UpdateGuildEmblemRequest request);
        Task SendGuildDisband(ProtoService.GuildDisbandRequest request);
        Task SendChangePlayerGuildRank(ProtoService.UpdateGuildMemberRankRequest request);
        Task SendGuildExpelMember(ProtoService.ExpelFromGuildRequest expelFromGuildRequest);
        Task SendPlayerLeaveGuild(ProtoService.LeaveGuildRequest leaveGuildRequest);
        Task SendPlayerJoinGuild(ProtoService.JoinGuildRequest joinGuildRequest);


        ProtoService.GetAllianceResponse GetAlliance(int id);
        ProtoService.CreateAllianceCheckResponse CreateAllianceCheck(ProtoService.CreateAllianceCheckRequest request);
        Task CreateAlliance(ProtoService.CreateAllianceRequest request);
        Task SendGuildLeaveAlliance(ProtoService.GuildLeaveAllianceRequest guildLeaveAllianceRequest);
        Task SendAllianceExpelGuild(ProtoService.AllianceExpelGuildRequest allianceExpelGuildRequest);
        Task SendChangeAllianceLeader(ProtoService.AllianceChangeLeaderRequest allianceChangeLeaderRequest);
        Task SendChangePlayerAllianceRank(ProtoService.ChangePlayerAllianceRankRequest changePlayerAllianceRankRequest);
        Task SendIncreaseAllianceCapacity(ProtoService.IncreaseAllianceCapacityRequest increaseAllianceCapacityRequest);
        Task SendUpdateAllianceRankTitle(ProtoService.UpdateAllianceRankTitleRequest request);
        Task SendUpdateAllianceNotice(ProtoService.UpdateAllianceNoticeRequest updateAllianceNoticeRequest);
        Task SendAllianceDisband(ProtoService.DisbandAllianceRequest disbandAllianceRequest);
        Task AllianceBroadcastPlayerInfo(ProtoService.AllianceBroadcastPlayerInfoRequest request);
        #endregion

        Task SendPlayerJoinChatRoom(ProtoService.JoinChatRoomRequest joinChatRoomRequest);
        Task SendPlayerLeaveChatRoom(ProtoService.LeaveChatRoomRequest leaveChatRoomRequst);
        Task SendChatRoomMesage(ProtoService.SendChatRoomMessageRequest sendChatRoomMessageRequest);
        Task SendCreateChatRoom(ProtoService.CreateChatRoomRequest createChatRoomRequest);

        Task SendInvitation(ProtoService.CreateInviteRequest request);
        Task AnswerInvitation(ProtoService.AnswerInviteRequest request);

        void RegisterExpedition(ProtoModel.ExpeditionRegistry request);
        ProtoService.ExpeditionCheckResponse CanStartExpedition(ProtoService.ExpeditionCheckRequest expeditionCheckRequest);

        Task ReceiveNewYearCard(ProtoService.ReceiveNewYearCardRequest receiveNewYearCardRequest);
        Task SendNewYearCard(ProtoService.SendNewYearCardRequest sendNewYearCardRequest);
        Task SendDiscardNewYearCard(ProtoService.DiscardNewYearCardRequest discardNewYearCardRequest);

        ProtoService.SetFlyResponse SendSetFly(ProtoService.SetFlyRequest setFlyRequest);
        Task SendReloadEvents(ProtoService.ReloadEventsRequest reloadEventsRequest);
        ProtoService.CreateTVMessageResponse BroadcastTV(ProtoService.CreateTVMessageRequest request);
        ProtoService.UseItemMegaphoneResponse SendItemMegaphone(ProtoService.UseItemMegaphoneRequest request);
        ProtoModel.DropAllProto RequestDropData();
        ProtoService.QueryMonsterCardDataResponse RequestMonsterCardData();
        ProtoService.QueryRankedGuildsResponse RequestRankedGuilds();
        ProtoService.GetAllPLifeResponse GetAllPLife(ProtoService.GetAllPLifeRequest request);
        ProtoService.GetPLifeByMapIdResponse RequestPLifeByMapId(ProtoService.GetPLifeByMapIdRequest requestPLifeByMapIdRequest);
        Task SendCreatePLife(ProtoService.CreatePLifeRequest createPLifeRequest);
        Task SendRemovePLife(ProtoService.RemovePLifeRequest removePLifeRequest);
        ProtoService.BuyCashItemResponse SendBuyCashItem(ProtoService.BuyCashItemRequest buyCashItemRequest);

        ProtoModel.RemoteHiredMerchantProto LoadPlayerHiredMerchant(ProtoService.GetPlayerHiredMerchantRequest getPlayerShopRequest);
        void SyncPlayerShop(ProtoService.SyncPlayerShopRequest request);
        ProtoService.CommitRetrievedResponse CommitRetrievedFromFredrick(ProtoService.CommitRetrievedRequest commitRetrievedRequest);
        Task BatchSyncPlayerShop(ProtoService.BatchSyncPlayerShopRequest request);

        Task BatchSyncMap(List<ProtoModel.MapSyncProto> data);
        Task SendReport(ProtoService.SendReportRequest sendReportRequest);
        Task SetMonitor(ProtoService.ToggleMonitorPlayerRequest toggleMonitorPlayerRequest);
        ProtoModel.MonitorDataWrapperProto LoadMonitor();
        Task SetAutoBanIgnored(ProtoService.ToggleAutoBanIgnoreRequest toggleAutoBanIgnoreRequest);
        ProtoModel.AutoBanIgnoredWrapperProto LoadAutobanIgnoreData();

        Task AntiMacroNotify(ProtoModel.AntiMacroNotifyMessageProto message);
        Task Ban(ProtoService.BanRequest banRequest);
        Task Unban(ProtoService.UnbanRequest unbanRequest);
        Task SetGmLevel(ProtoService.SetGmLevelRequest setGmLevelRequest);
        ProtoService.ShowOnlinePlayerResponse GetOnlinedPlayers();
        Task WarpPlayerByName(ProtoService.WrapPlayerByNameRequest wrapPlayerByNameRequest);
        Task SummonPlayerByName(ProtoService.SummonPlayerByNameRequest summonPlayerByNameRequest);
        Task DisconnectPlayerByName(ProtoService.DisconnectPlayerByNameRequest disconnectPlayerByNameRequest);
        ProtoModel.GetAllClientInfo GetOnliendClientInfo();
        Task ShutdownMaster(ProtoService.ShutdownMasterRequest shutdownMasterRequest);
        Task CompleteChannelShutdown();
        ProtoModel.ServerStateProto GetServerState();

        ProtoModel.GacheponDataProto GetGachaponData();
        ProtoService.NameChangeResponse ReigsterNameChange(ProtoService.NameChangeRequest nameChangeRequest);
        Task SyncPlayer(ProtoModel.PlayerSaveProto data, SyncCharacterTrigger trigger = SyncCharacterTrigger.Unknown, bool saveDB = false);
        Task BatchSyncPlayer(List<ProtoModel.PlayerSaveProto> data, bool saveDB = false);
        #region Buddy
        Task SendAddBuddyRequest(ProtoService.AddBuddyRequest request);
        Task SendAddBuddyRequest(ProtoService.AddBuddyByIdRequest request);
        Task SendBuddyMessage(ProtoModel.SendBuddyNoticeMessageProto request);
        Task SendDeleteBuddy(ProtoService.DeleteBuddyRequest deleteBuddyRequest);

        Task GetLocation(ProtoService.GetLocationRequest getLocationRequest);
        #endregion

        Task SendWhisper(ProtoService.SendWhisperMessageRequest sendWhisperMessageRequest);


        ProtoService.UseCdkResponse UseCdk(ProtoService.UseCdkRequest useCdkRequest);
        bool GainCharacterSlot(int accountId);
        Task SendGuildPacket(ProtoService.GuildPacketRequest guildPacketRequest);
        Task SendMultiChatAsync(int type, string fromName, string msg, int[] receivers);
        Task SaveAllNotifyAsync();
        Task DisconnectAllNotifyAsync();

        #region Duey
        Task<ProtoService.CreatePackageResponse> CreateDueyPackage(ProtoService.CreatePackageRequest request);
        Task TakeDueyPackage(ProtoService.TakeDueyPackageRequest request);
        Task RequestRemovePackage(ProtoService.RemovePackageRequest request);
        Task GetDueyPackagesByPlayerId(ProtoService.GetPlayerDueyPackageRequest request);
        Task TakeDueyPackageCommit(ProtoService.TakeDueyPackageCommitRequest takeDueyPackageCommit);
        #endregion

        Task JailPlayer(ProtoService.CreateJailRequest request);
        Task UnjailPlayer(ProtoService.CreateUnjailRequest request);

        Task SendRemoveDoor(int ownerId);
        Task<ProtoModel.GetRewardsResponse> GetActiveRewards(ProtoModel.GetRewardsRequest request);
        Task<ProtoService.UseCdkResponse> TakeReward(ProtoService.UseIdRequest request);
    }
}
