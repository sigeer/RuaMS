using AllianceProto;
using Application.Core.Channel;
using Application.Core.Channel.Internal;
using Application.Shared.Events;
using Application.Shared.Login;
using Application.Shared.Message;
using Application.Shared.Servers;
using Application.Shared.Team;
using BaseProto;
using CashProto;
using Config;
using ConfigProto;
using CreatorProto;
using Dto;
using ExpeditionProto;
using Google.Protobuf.WellKnownTypes;
using GuildProto;
using InvitationProto;
using ItemProto;
using LifeProto;
using MessageProto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RankProto;
using ServerProto;
using ServiceProto;
using SyncProto;
using System.Net;
using SystemProto;
using TeamProto;

namespace Application.Core.ServerTransports
{
    public class DefaultChannelServerTransport : IChannelServerTransport
    {
        readonly ServiceProto.SystemService.SystemServiceClient _systemClient;
        readonly ServiceProto.GameService.GameServiceClient _gameClient;
        readonly ServiceProto.SyncService.SyncServiceClient _syncClient;
        readonly ServiceProto.GuildService.GuildServiceClient _guildClient;
        readonly ServiceProto.AllianceService.AllianceServiceClient _allianceClient;
        readonly ServiceProto.DataService.DataServiceClient _dataClient;
        readonly ServiceProto.ItemService.ItemServiceClient _itemClient;
        readonly ServiceProto.CashService.CashServiceClient _cashClient;
        readonly ServiceProto.TeamService.TeamServiceClient _teamClient;
        readonly ServiceProto.BuddyService.BuddyServiceClient _buddyClient;
        readonly ServiceProto.PlayerShopService.PlayerShopServiceClient _playerShopClient;

        readonly ChannelServerConfig _config;
        IServiceProvider _sp;
        public DefaultChannelServerTransport(
            IServiceProvider sp,
            IOptions<ChannelServerConfig> options,
            SystemService.SystemServiceClient systemClient,
            GameService.GameServiceClient gameClient,
            SyncService.SyncServiceClient syncClient,
            GuildService.GuildServiceClient guildClient,
            AllianceService.AllianceServiceClient allianceClient,
            DataService.DataServiceClient dataClient,
            ItemService.ItemServiceClient itemClient,
            CashService.CashServiceClient cashClient,
            TeamService.TeamServiceClient teamClient,
            BuddyService.BuddyServiceClient buddyClient,
            PlayerShopService.PlayerShopServiceClient playerShopClient)
        {
            _sp = sp;
            _config = options.Value;
            _systemClient = systemClient;
            _gameClient = gameClient;
            _syncClient = syncClient;
            _guildClient = guildClient;
            _allianceClient = allianceClient;
            _dataClient = dataClient;
            _itemClient = itemClient;
            _cashClient = cashClient;
            _teamClient = teamClient;
            _buddyClient = buddyClient;
            _playerShopClient = playerShopClient;
        }

        public long GetCurrentTime()
        {
            return _systemClient.GetCurrentTime(new Google.Protobuf.WellKnownTypes.Empty()).Value;
        }

        public int GetCurrentTimestamp()
        {
            return (int)_systemClient.GetCurrentTimestamp(new Google.Protobuf.WellKnownTypes.Empty()).Value;
        }

        public void BroadcastMessage(MessageProto.PacketRequest p)
        {
            _systemClient.BroadcastMessage(p);
        }

        public void SendWorldConfig(Config.WorldConfig updatePatch)
        {
            _systemClient.SendWorldConfig(updatePatch);
        }

        public IPEndPoint GetChannelEndPoint(int channel)
        {
            var res = _systemClient.GetChannelEndPoint(new BaseProto.GetChannelEndPointRequest { Channel = channel });
            return new IPEndPoint(new IPAddress(res.Address.ToByteArray()), res.Port);
        }

        public SyncProto.PlayerGetterDto GetPlayerData(string clientSession, int cid)
        {
            return _syncClient.GetPlayerObject(new SyncProto.GetPlayerByLoginRequest { ClientSession = clientSession, CharacterId = cid }).Data;
        }

        public void SendTimer(int seconds)
        {
            _systemClient.SendTimer(new SetTimer { Seconds = seconds });
        }
        public void RemoveTimer()
        {
            _systemClient.RemoveTimer(new Empty());
        }

        public async Task RegisterServer(List<ChannelConfig> channels, CancellationToken cancellationToken)
        {
            var server = _sp.GetRequiredService<WorldChannelServer>();
            var streamingCall = _systemClient.Connect();
            server.InternalSession.Connect(streamingCall);

            var req = new RegisterServerRequest { ServerName = _config.ServerName, ServerHost = _config.ServerHost, GrpcUrl = _config.GrpcUrl };
            req.Channels.AddRange(channels.Select(x => new RegisterChannelConfigDto { Port = x.Port, MaxSize = x.MaxSize }));

            await server.InternalSession.SendAsync(ChannelSendCode.RegisterChannel, req, cancellationToken);
        }

        public async Task CompleteChannelShutdown()
        {
            var server = _sp.GetRequiredService<WorldChannelServer>();
            await server.InternalSession.DisconnectAsync();
        }

        public void DropWorldMessage(DropMessageRequest request)
        {
            _systemClient.DropMessage(request);
        }

        public void RemoveGuildQueued(int guildId)
        {
            _dataClient.RemoveGuildQueued(new QuildRequest { GuildId = guildId });
        }

        public bool IsGuildQueued(int guildId)
        {
            return _dataClient.IsGuildQueued(new QuildRequest { GuildId = guildId }).Value;
        }

        public void PutGuildQueued(int guildId)
        {
            _dataClient.PutGuildQueued(new QuildRequest { GuildId = guildId });
        }

        public TeamDto CreateTeam(int playerId)
        {
            return _teamClient.CreateTeam(new CreateTeamRequest { LeaderId = playerId }).Model;
        }

        public SearchHiredMerchantChannelResponse FindPlayerShopChannel(SearchHiredMerchantChannelRequest request)
        {
            return _playerShopClient.FindPlayerShopChannel(request);
        }

        public void SendAccountLogout(int accountId)
        {
            _syncClient.UpdateAccountState(new UpdateAccountStateRequest { AccId = accountId, State = LoginStage.LOGIN_NOTLOGGEDIN });
        }

        public AccountLoginStatus UpdateAccountState(int accId, sbyte state)
        {
            var res = _syncClient.UpdateAccountState(new UpdateAccountStateRequest { AccId = accId, State = state });
            return new AccountLoginStatus(res.State, res.Time.ToDateTimeOffset());
        }

        public void SetCharacteridInTransition(string v, int cid)
        {
            _syncClient.SetCharacterTransition(new SetClientCharacterTransitionRequest { CharacterId = cid, ClientSession = v });
        }

        public bool HasCharacteridInTransition(string clientSession)
        {
            return _syncClient.HasCharacterInTransition(new CheckCharacterInTransitionRequest { ClientSession = clientSession }).Value;
        }

        public bool CheckCharacterName(string name)
        {
            return _gameClient.CheckCharacterName(new ServiceProto.CheckCharacterNameRequest { Name = name }).Value;
        }

        public void SendBuffObject(int v, PlayerBuffDto playerBuffSaveDto)
        {
            _syncClient.PushPlayerBuffers(new PushPlayerBuffsRequest { CharacterId = v, Data = playerBuffSaveDto });
        }

        public PlayerBuffDto GetBuffObject(int id)
        {
            return _syncClient.GetPlayerBuffers(new GetPlayerBufferRequest { CharacterId = id });
        }

        public void SetPlayerOnlined(int id, int channelId)
        {
            _syncClient.CompleteLogin(new ServiceProto.CompleteLoginRequest { CharacterId = id, Channel = channelId });
        }

        public DropAllDto RequestAllReactorDrops()
        {
            return _gameClient.LoadReactorDropData(new Empty());
        }

        public int[] RequestReactorSkillBooks()
        {
            return _gameClient.LoadReactorSkillBookData(new Empty()).IdList.ToArray();
        }

        public SpecialCashItemListDto RequestSpecialCashItems()
        {
            return _cashClient.LoadSpecialItems(new Empty());
        }

        public GetMyGiftsResponse LoadPlayerGifts(GetMyGiftsRequest request)
        {
            return _cashClient.LoadGifts(request);
        }

        public void ClearGifts(int[] giftIdArray)
        {
            var req = new ItemProto.CommitRetrieveGiftRequest();
            req.IdList.AddRange(giftIdArray);
            _cashClient.CommitRetrieveGift(req);
        }

        public bool SendNormalNoteMessage(int senderId, string toName, string noteMessage)
        {
            return _gameClient.SendNote(new SendNormalNoteRequest { FromId = senderId, Message = noteMessage, ToName = toName }).Value;
        }

        public NoteDto? DeleteNoteMessage(int id)
        {
            return _gameClient.SetNoteRead(new SetNoteReadRequest { Id = id }).Data;
        }

        public ShopDto? GetShop(int id, bool isShopId)
        {
            return _gameClient.GetShop(new GetShopRequest { Id = id, IsShopId = isShopId }).Data;
        }

        public LoadCharacterRankResponse LoadPlayerRanking(int topCount)
        {
            return _gameClient.LoadCharacterRank(new LoadCharacterRankRequest { Count = topCount });
        }

        public void SendToggleCoupon(int itemId)
        {
            _gameClient.SendToggleCoupon(new ToggelCouponRequest { Id = itemId });
        }

        public CreateCharResponseDto SendNewPlayer(NewPlayerSaveDto data)
        {
            return _syncClient.CreateCharacter(data);
        }

        public CreateCharCheckResponse CreatePlayerCheck(CreateCharCheckRequest request)
        {
            return _syncClient.CreateCharacterCheck(request);
        }

        public int[][] GetMostSellerCashItems()
        {
            return _cashClient.LoadMosterSellItems(new Empty()).Tabs.Select(x => x.ItemIdList.ToArray()).ToArray();
        }

        public OwlSearchRecordResponse GetOwlSearchedItems()
        {
            return _itemClient.LoadOwlSearchRecords(new Empty());
        }

        public OwlSearchResponse SendOwlSearch(OwlSearchRequest owlSearchRequest)
        {
            return _itemClient.UseOwlSearch(owlSearchRequest);
        }
        public UpdateTeamResponse SendUpdateTeam(int teamId, PartyOperation operation, int fromId, int toId)
        {
            return _teamClient.SendTeamUpdate(new UpdateTeamRequest { FromId = fromId, Operation = (int)operation, TargetId = toId, TeamId = teamId });
        }
        public GetTeamResponse GetTeam(int party)
        {
            return _teamClient.GetTeamModel(new GetTeamRequest { Id = party });
        }

        public GetGuildResponse GetGuild(int id)
        {
            return _guildClient.GetGuildModel(new GetGuildRequest { Id = id });
        }

        public GetGuildResponse CreateGuild(string guildName, int playerId, int[] members)
        {
            var req = new CreateGuildRequest { LeaderId = playerId, Name = guildName };
            req.Members.AddRange(members);
            return _guildClient.CreateGuild(req);
        }

        public void BroadcastGuildMessage(int guildId, int v, string callout)
        {
            _guildClient.GuildDropMessage(new GuildDropMessageRequest { GuildId = guildId, Type = v, Message = callout });
        }




        public void SendUpdateGuildGP(UpdateGuildGPRequest request)
        {
            _guildClient.UpdateGP(request);
        }

        public void SendUpdateGuildRankTitle(UpdateGuildRankTitleRequest request)
        {
            _guildClient.UpdateRankTitle(request);
        }

        public void SendUpdateGuildNotice(UpdateGuildNoticeRequest request)
        {
            _guildClient.UpdateNotice(request);
        }

        public void SendUpdateGuildCapacity(UpdateGuildCapacityRequest request)
        {
            _guildClient.UpdateCapacity(request);
        }

        public void SendUpdateGuildEmblem(UpdateGuildEmblemRequest request)
        {
            _guildClient.UpdateEmblem(request);
        }

        public void SendGuildDisband(GuildDisbandRequest request)
        {
            _guildClient.DisbandGuild(request);
        }

        public void SendChangePlayerGuildRank(UpdateGuildMemberRankRequest request)
        {
            _guildClient.UpdateMemberRank(request);
        }

        public void SendGuildExpelMember(ExpelFromGuildRequest expelFromGuildRequest)
        {
            _guildClient.ExpelFromGuild(expelFromGuildRequest);
        }

        public void SendPlayerLeaveGuild(LeaveGuildRequest leaveGuildRequest)
        {
            _guildClient.LeaveGuild(leaveGuildRequest);
        }

        public void SendPlayerJoinGuild(JoinGuildRequest joinGuildRequest)
        {
            _guildClient.JoinGuild(joinGuildRequest);
        }

        public GetAllianceResponse GetAlliance(int id)
        {
            return _allianceClient.GetAllianceModel(new GetAllianceRequest { Id = id });
        }

        public CreateAllianceCheckResponse CreateAllianceCheck(CreateAllianceCheckRequest request)
        {
            return _allianceClient.CreateAllianceCheck(request);
        }

        public GetAllianceResponse CreateAlliance(int[] masters, string allianceName)
        {
            var req = new CreateAllianceRequest() { Name = allianceName };
            req.Members.AddRange(masters);
            return _allianceClient.CreateAlliance(req);
        }

        public void SendGuildJoinAlliance(GuildJoinAllianceRequest guildJoinAllianceRequest)
        {
            _allianceClient.JoinAlliance(guildJoinAllianceRequest);
        }

        public void SendGuildLeaveAlliance(GuildLeaveAllianceRequest guildLeaveAllianceRequest)
        {
            _allianceClient.LeavelAlliance(guildLeaveAllianceRequest);
        }

        public void SendAllianceExpelGuild(AllianceExpelGuildRequest allianceExpelGuildRequest)
        {
            _allianceClient.AllianceExpelGuild(allianceExpelGuildRequest);
        }

        public void SendChangeAllianceLeader(AllianceChangeLeaderRequest allianceChangeLeaderRequest)
        {
            _allianceClient.ChangeLeader(allianceChangeLeaderRequest);
        }

        public void SendChangePlayerAllianceRank(ChangePlayerAllianceRankRequest changePlayerAllianceRankRequest)
        {
            _allianceClient.ChangeMemberRank(changePlayerAllianceRankRequest);
        }

        public void SendIncreaseAllianceCapacity(IncreaseAllianceCapacityRequest increaseAllianceCapacityRequest)
        {
            _allianceClient.IncreaseAllianceCapacity(increaseAllianceCapacityRequest);
        }

        public void SendUpdateAllianceRankTitle(UpdateAllianceRankTitleRequest request)
        {
            _allianceClient.UpdateRankTitle(request);
        }

        public void SendUpdateAllianceNotice(UpdateAllianceNoticeRequest updateAllianceNoticeRequest)
        {
            _allianceClient.UpdateNotice(updateAllianceNoticeRequest);
        }

        public void SendAllianceDisband(DisbandAllianceRequest disbandAllianceRequest)
        {
            _allianceClient.DisbandAlliance(disbandAllianceRequest);
        }

        public void SendPlayerJoinChatRoom(JoinChatRoomRequest joinChatRoomRequest)
        {
            _gameClient.JoinChatRoom(joinChatRoomRequest);
        }

        public void SendPlayerLeaveChatRoom(LeaveChatRoomRequst leaveChatRoomRequst)
        {
            _gameClient.LeaveChatRoom(leaveChatRoomRequst);
        }

        public void SendChatRoomMesage(SendChatRoomMessageRequest sendChatRoomMessageRequest)
        {
            _gameClient.SendChatRoomMessage(sendChatRoomMessageRequest);
        }

        public void SendCreateChatRoom(CreateChatRoomRequest createChatRoomRequest)
        {
            _gameClient.CreateChatRoom(createChatRoomRequest);
        }

        public void SendInvitation(CreateInviteRequest request)
        {
            _gameClient.SendInvitation(request);
        }

        public void AnswerInvitation(AnswerInviteRequest request)
        {
            _gameClient.AnswerInvitation(request);
        }

        public void RegisterExpedition(ExpeditionRegistry request)
        {
            _gameClient.RegisterExpedition(request);
        }

        public ExpeditionCheckResponse CanStartExpedition(ExpeditionCheckRequest expeditionCheckRequest)
        {
            return _gameClient.CheckExpedition(expeditionCheckRequest);
        }

        public QueryChannelExpedtionResponse GetExpeditionInfo()
        {
            return _gameClient.GetExpeditionInfo(new Empty());
        }

        public void ReceiveNewYearCard(ReceiveNewYearCardRequest receiveNewYearCardRequest)
        {
            _gameClient.RecevieNewyearCard(receiveNewYearCardRequest);
        }

        public void SendNewYearCard(SendNewYearCardRequest sendNewYearCardRequest)
        {
            _gameClient.SendNewyearCard(sendNewYearCardRequest);
        }

        public void SendDiscardNewYearCard(DiscardNewYearCardRequest discardNewYearCardRequest)
        {
            _gameClient.DiscardNewyearCard(discardNewYearCardRequest);
        }

        public SetFlyResponse SendSetFly(SetFlyRequest setFlyRequest)
        {
            return _systemClient.SetAccountFly(setFlyRequest);
        }

        public void SendReloadEvents(ReloadEventsRequest reloadEventsRequest)
        {
            _gameClient.ReloadEvents(reloadEventsRequest);
        }

        public CreateTVMessageResponse BroadcastTV(CreateTVMessageRequest request)
        {
            return _itemClient.UseTV(request);
        }

        public UseItemMegaphoneResponse SendItemMegaphone(UseItemMegaphoneRequest request)
        {
            return _itemClient.UseItemMegaphone(request);
        }

        public DropAllDto RequestDropData()
        {
            return _gameClient.LoadMobDropData(new Empty());
        }

        public QueryMonsterCardDataResponse RequestMonsterCardData()
        {
            return _gameClient.LoadMonsterCardData(new Empty());
        }

        public QueryRankedGuildsResponse RequestRankedGuilds()
        {
            return _guildClient.GetGuildRank(new Empty());
        }

        public LifeProto.GetPLifeByMapIdResponse RequestPLifeByMapId(LifeProto.GetPLifeByMapIdRequest requestPLifeByMapIdRequest)
        {
            return _gameClient.GetLifeByMapId(requestPLifeByMapIdRequest);
        }

        public GetAllPLifeResponse GetAllPLife(GetAllPLifeRequest request)
        {
            return _gameClient.GetAllPLife(request);
        }

        public void SendCreatePLife(LifeProto.CreatePLifeRequest createPLifeRequest)
        {
            _gameClient.CreateLife(createPLifeRequest);
        }

        public void SendRemovePLife(LifeProto.RemovePLifeRequest removePLifeRequest)
        {
            _gameClient.RemoveLife(removePLifeRequest);
        }

        public BuyCashItemResponse SendBuyCashItem(BuyCashItemRequest buyCashItemRequest)
        {
            return _cashClient.BuyCashItem(buyCashItemRequest);
        }

        public RemoteHiredMerchantDto LoadPlayerHiredMerchant(GetPlayerHiredMerchantRequest getPlayerShopRequest)
        {
            return _gameClient.LoadPlayerHiredMerchant(getPlayerShopRequest);
        }

        public void SyncPlayerShop(SyncPlayerShopRequest request)
        {
            _syncClient.SyncPlayerShop(request);
        }

        public CommitRetrievedResponse CommitRetrievedFromFredrick(CommitRetrievedRequest commitRetrievedRequest)
        {
            return _gameClient.CommitRetrievedFromFredrick(commitRetrievedRequest);
        }

        public CanHiredMerchantResponse CanHiredMerchant(CanHiredMerchantRequest canHiredMerchantRequest)
        {
            return _gameClient.CanHiredMerchant(canHiredMerchantRequest);
        }

        public void BatchSyncPlayerShop(BatchSyncPlayerShopRequest request)
        {
            _syncClient.BatchSyncPlayerShop(request);
        }

        public StoreItemsResponse SaveItems(StoreItemsRequest request)
        {
            return _itemClient.SaveItems(request);
        }

        public LoadItemsFromStoreResponse LoadItemFromStore(LoadItemsFromStoreRequest loadItemsFromStoreRequest)
        {
            return _itemClient.LoadItemFromStore(loadItemsFromStoreRequest);
        }

        public void BatchSyncMap(List<MapSyncDto> data)
        {
            var req = new MapBatchSyncDto();
            req.List.AddRange(data);
            _syncClient.BatchSyncMap(req);
        }

        public SendReportResponse SendReport(SendReportRequest sendReportRequest)
        {
            return _systemClient.SendReport(sendReportRequest);
        }

        public ToggleMonitorPlayerResponse SetMonitor(ToggleMonitorPlayerRequest toggleMonitorPlayerRequest)
        {
            return _gameClient.SetMonitor(toggleMonitorPlayerRequest);
        }

        public MonitorDataWrapper LoadMonitor()
        {
            return _gameClient.LoadMonitor(new Empty());
        }

        public ToggleAutoBanIgnoreResponse SetAutoBanIgnored(ToggleAutoBanIgnoreRequest toggleAutoBanIgnoreRequest)
        {
            return _systemClient.SetAutobanIgnore(toggleAutoBanIgnoreRequest);
        }

        public AutoBanIgnoredWrapper LoadAutobanIgnoreData()
        {
            return _systemClient.GetAutobanIgnores(new Empty());
        }

        public BanResponse Ban(BanRequest banRequest)
        {
            return _systemClient.Ban(banRequest);
        }

        public UnbanResponse Unban(UnbanRequest unbanRequest)
        {
            return _systemClient.Unban(unbanRequest);
        }

        public SetGmLevelResponse SetGmLevel(SetGmLevelRequest setGmLevelRequest)
        {
            return _systemClient.SetGmLevel(setGmLevelRequest);
        }

        public ShowOnlinePlayerResponse GetOnlinedPlayers()
        {
            return _systemClient.GetOnlinedPlayers(new Empty());
        }

        public WrapPlayerByNameResponse WarpPlayerByName(WrapPlayerByNameRequest wrapPlayerByNameRequest)
        {
            return _systemClient.WrapPlayer(wrapPlayerByNameRequest);
        }

        public SummonPlayerByNameResponse SummonPlayerByName(SummonPlayerByNameRequest summonPlayerByNameRequest)
        {
            return _systemClient.SummonPlayer(summonPlayerByNameRequest);
        }

        public DisconnectPlayerByNameResponse DisconnectPlayerByName(DisconnectPlayerByNameRequest disconnectPlayerByNameRequest)
        {
            return _systemClient.DisconnectPlayer(disconnectPlayerByNameRequest);
        }

        public void DisconnectAll(DisconnectAllRequest disconnectAllRequest)
        {
            _systemClient.DisconnectAll(disconnectAllRequest);
        }

        public GetAllClientInfo GetOnliendClientInfo()
        {
            return _systemClient.GetOnlinedClients(new Empty());
        }

        public void ShutdownMaster(ShutdownMasterRequest shutdownMasterRequest)
        {
            _systemClient.ShutdownMaster(shutdownMasterRequest);
        }



        public void SaveAll(Empty empty)
        {
            _systemClient.SaveAll(new Empty());
        }

        public ServerStateDto GetServerState()
        {
            return _systemClient.GetServerState(new Empty());
        }

        public GacheponDataDto GetGachaponData()
        {
            return _gameClient.LoadGachaponData(new Empty());
        }

        public NameChangeResponse ReigsterNameChange(NameChangeRequest nameChangeRequest)
        {
            return _gameClient.ChangeName(nameChangeRequest);
        }

        public void BatchSyncPlayer(List<PlayerSaveDto> data, bool saveDB = false)
        {
            var req = new SyncProto.BatchSyncPlayerRequest() { SaveDb = saveDB};
            req.List.AddRange(data);
            _syncClient.BatchPushCharacter(req);
        }


        public void SyncPlayer(PlayerSaveDto data, SyncCharacterTrigger trigger = SyncCharacterTrigger.Unknown, bool saveDB = false)
        {
            _syncClient.PushCharacter(new SyncPlayerRequest { Trigger = (int)trigger, Data = data, SaveDb = saveDB });
        }
        public AddBuddyResponse SendAddBuddyRequest(AddBuddyRequest request)
        {
            return _buddyClient.AddBuddyByName(request);
        }

        public AddBuddyResponse SendAddBuddyRequest(AddBuddyByIdRequest request)
        {
            return _buddyClient.AddBuddyById(request);
        }

        public void SendBuddyMessage(SendBuddyNoticeMessageDto request)
        {
            _buddyClient.SendBuddfyNotice(request);
        }

        public DeleteBuddyResponse SendDeleteBuddy(DeleteBuddyRequest deleteBuddyRequest)
        {
            return _buddyClient.DeleteBuddy(deleteBuddyRequest);
        }

        public SendWhisperMessageResponse SendWhisper(SendWhisperMessageRequest sendWhisperMessageRequest)
        {
            return _buddyClient.SendWhisper(sendWhisperMessageRequest);
        }

        public GetLocationResponse GetLocation(GetLocationRequest getLocationRequest)
        {
            return _buddyClient.GetLocation(getLocationRequest);
        }

        public void SendYellowTip(YellowTipRequest yellowTipRequest)
        {
            _gameClient.SendYellowTip(yellowTipRequest);
        }

        public UseCdkResponse UseCdk(UseCdkRequest useCdkRequest)
        {
            return _gameClient.UseCDK(useCdkRequest);
        }

        public void HealthCheck(MonitorData data)
        {
            _systemClient.HealthCheck(data);
        }

        public void SendEarnTitleMessage(EarnTitleMessageRequest data)
        {
            _gameClient.SendEarnTitleMessage(data);
        }

        public bool GainCharacterSlot(int accountId)
        {
            return _systemClient.GainCharacterSlot(new GainAccountCharacterSlotRequest { AccId = accountId }).Code == 0;
        }

        public void SendGuildPacket(GuildPacketRequest guildPacketRequest)
        {
            _guildClient.SendGuildPacket(guildPacketRequest);
        }
    }
}
