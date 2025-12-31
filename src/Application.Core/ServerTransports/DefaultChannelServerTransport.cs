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
using DueyDto;
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
using server.quest;
using ServerProto;
using ServiceProto;
using SyncProto;
using System.Net;
using System.Threading.Tasks;
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
        readonly ServiceProto.PlayerShopService.PlayerShopServiceClient _playerShopClient;

        Lazy<InternalSession> _internalSession;
        public InternalSession InternalSession => _internalSession.Value;

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
            _playerShopClient = playerShopClient;

            _internalSession = new Lazy<InternalSession>(() => new InternalSession(_sp.GetRequiredService<WorldChannelServer>()));
        }

        public long GetCurrentTime()
        {
            return _systemClient.GetCurrentTime(new Google.Protobuf.WellKnownTypes.Empty()).Value;
        }

        public int GetCurrentTimestamp()
        {
            return (int)_systemClient.GetCurrentTimestamp(new Google.Protobuf.WellKnownTypes.Empty()).Value;
        }

        public async Task BroadcastMessage(MessageProto.PacketRequest p)
        {
            await InternalSession.SendAsync(ChannelSendCode.BroadcastPacket, p);
        }

        public async Task SendWorldConfig(Config.WorldConfig updatePatch)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateWorldConfig, updatePatch);
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

        public async Task SendTimer(int seconds)
        {
            await InternalSession.SendAsync(ChannelSendCode.SetTimer, new SetTimer { Seconds = seconds });
        }
        public async Task RemoveTimer()
        {
            await InternalSession.SendAsync(ChannelSendCode.RemoveTimer);
        }

        public async Task RegisterServer(List<ChannelConfig> channels, CancellationToken cancellationToken)
        {
            var streamingCall = _systemClient.Connect();
            InternalSession.Connect(streamingCall);

            var req = new RegisterServerRequest { ServerName = _config.ServerName, ServerHost = _config.ServerHost };
            req.Channels.AddRange(channels.Select(x => new RegisterChannelConfigDto { Port = x.Port, MaxSize = x.MaxSize }));

            await InternalSession.SendAsync(ChannelSendCode.RegisterChannel, req, cancellationToken);
        }

        public async Task CreatePlayerResponseAsync(CreateCharResponseDto res, CancellationToken cancellationToken)
        {
            await InternalSession.SendAsync(ChannelSendCode.CreateCharacterResponse, res, cancellationToken);
        }

        public async Task CompleteChannelShutdown()
        {
            await InternalSession.DisconnectAsync();
        }

        public async Task DropWorldMessage(DropMessageRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.DropMessage, request);
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

        public async Task SetPlayerOnlined(int id, int channelId)
        {
            await _syncClient.CompleteLoginAsync(new ServiceProto.CompleteLoginRequest { CharacterId = id, Channel = channelId });
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

        public async Task SendToggleCoupon(int itemId)
        {
            await InternalSession.SendAsync(ChannelSendCode.ToggleCoupon, new ToggelCouponRequest { Id = itemId });
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
        public async Task SendUpdateTeam(int teamId, PartyOperation operation, int fromId, int toId)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateTeam, new UpdateTeamRequest { FromId = fromId, Operation = (int)operation, TargetId = toId, TeamId = teamId });
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




        public async Task SendUpdateGuildGP(UpdateGuildGPRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateGuildGp, request);
        }

        public async Task SendUpdateGuildRankTitle(UpdateGuildRankTitleRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateGuildRankTitle, request);
        }

        public async Task SendUpdateGuildNotice(UpdateGuildNoticeRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateGuildNotice, request);
        }

        public async Task SendUpdateGuildCapacity(UpdateGuildCapacityRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateGuildCapacity, request);
        }

        public async Task SendUpdateGuildEmblem(UpdateGuildEmblemRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateGuildEmblem, request);
        }

        public async Task SendGuildDisband(GuildDisbandRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.DisbandGuild, request);
        }

        public async Task SendChangePlayerGuildRank(UpdateGuildMemberRankRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.ChangeGuildMemberRank, request);
        }

        public async Task SendGuildExpelMember(ExpelFromGuildRequest expelFromGuildRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.ExpelGuildMember, expelFromGuildRequest);
        }

        public async Task SendPlayerLeaveGuild(LeaveGuildRequest leaveGuildRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.LeaveGuild, leaveGuildRequest);
        }

        public async Task SendPlayerJoinGuild(JoinGuildRequest joinGuildRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.JoinGuild, joinGuildRequest);
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

        public async Task SendGuildJoinAlliance(GuildJoinAllianceRequest guildJoinAllianceRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.JoinAlliance, guildJoinAllianceRequest);
        }

        public async Task SendGuildLeaveAlliance(GuildLeaveAllianceRequest guildLeaveAllianceRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.LeaveAlliance, guildLeaveAllianceRequest);
        }

        public async Task SendAllianceExpelGuild(AllianceExpelGuildRequest allianceExpelGuildRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.ExpelAllianceGuild, allianceExpelGuildRequest);
        }

        public async Task SendChangeAllianceLeader(AllianceChangeLeaderRequest allianceChangeLeaderRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateAllianceLeader, allianceChangeLeaderRequest);
        }

        public async Task SendChangePlayerAllianceRank(ChangePlayerAllianceRankRequest changePlayerAllianceRankRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateAllianceGuildRank, changePlayerAllianceRankRequest);
        }

        public async Task SendIncreaseAllianceCapacity(IncreaseAllianceCapacityRequest increaseAllianceCapacityRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateAllianceCapacity, increaseAllianceCapacityRequest);
        }

        public async Task SendUpdateAllianceRankTitle(UpdateAllianceRankTitleRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateAllianceRankTitle, request);
        }

        public async Task SendUpdateAllianceNotice(UpdateAllianceNoticeRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateAllianceNotice, request);
        }

        public async Task SendAllianceDisband(DisbandAllianceRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.DisbandAlliance, request);
        }

        public async Task SendPlayerJoinChatRoom(JoinChatRoomRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.JoinChatRoom, request);
        }

        public async Task SendPlayerLeaveChatRoom(LeaveChatRoomRequst request)
        {
            await InternalSession.SendAsync(ChannelSendCode.LeaveChatRoom, request);
        }

        public async Task SendChatRoomMesage(SendChatRoomMessageRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.SendChatRoomMessage, request);
        }

        public async Task SendCreateChatRoom(CreateChatRoomRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.CreateChatRoom, request);
        }

        public async Task SendInvitation(CreateInviteRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.SendInvitation, request);
        }

        public async Task AnswerInvitation(AnswerInviteRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.AnswerInvitation, request);
        }

        public void RegisterExpedition(ExpeditionRegistry request)
        {
            _gameClient.RegisterExpedition(request);
        }

        public ExpeditionCheckResponse CanStartExpedition(ExpeditionCheckRequest expeditionCheckRequest)
        {
            return _gameClient.CheckExpedition(expeditionCheckRequest);
        }

        public async Task ReceiveNewYearCard(ReceiveNewYearCardRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.ReceiveNewYearCard, request);
        }

        public async Task SendNewYearCard(SendNewYearCardRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.SendNewYearCard, request);
        }

        public async Task SendDiscardNewYearCard(DiscardNewYearCardRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.DiscardNewYearCard, request);
        }

        public SetFlyResponse SendSetFly(SetFlyRequest setFlyRequest)
        {
            return _systemClient.SetAccountFly(setFlyRequest);
        }

        public async Task SendReloadEvents(ReloadEventsRequest reloadEventsRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.ReloadWorldEvents, reloadEventsRequest);
        }

        public async Task BroadcastTV(CreateTVMessageRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.UseItemTV, request);
        }

        public async Task SendItemMegaphone(UseItemMegaphoneRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.UseItemMegaphone, request);
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

        public async Task SendCreatePLife(LifeProto.CreatePLifeRequest createPLifeRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.CreatePLife, createPLifeRequest);
        }

        public async Task SendRemovePLife(LifeProto.RemovePLifeRequest removePLifeRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.RemovePLife, removePLifeRequest);
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
            InternalSession.Send(ChannelSendCode.SyncMap, req);
        }

        public async Task SendReport(SendReportRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.SendReport, request);
        }

        public async Task SetMonitor(ToggleMonitorPlayerRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.SetMonitor, request);
        }

        public MonitorDataWrapper LoadMonitor()
        {
            return _gameClient.LoadMonitor(new Empty());
        }

        public async Task SetAutoBanIgnored(ToggleAutoBanIgnoreRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.SetAutobanIgnore, request);
        }

        public AutoBanIgnoredWrapper LoadAutobanIgnoreData()
        {
            return _systemClient.GetAutobanIgnores(new Empty());
        }

        public async Task Ban(BanRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.Ban, request);
        }

        public async Task Unban(UnbanRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.Unban, request);
        }

        public async Task SetGmLevel(SetGmLevelRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.SetGmLevel, request);
        }

        public ShowOnlinePlayerResponse GetOnlinedPlayers()
        {
            return _systemClient.GetOnlinedPlayers(new Empty());
        }

        public async Task WarpPlayerByName(WrapPlayerByNameRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.WarpPlayer, request);
        }

        public async Task SummonPlayerByName(SummonPlayerByNameRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.SummonPlayer, request);
        }

        public async Task DisconnectPlayerByName(DisconnectPlayerByNameRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.DisconnectOne, request);
        }

        public GetAllClientInfo GetOnliendClientInfo()
        {
            return _systemClient.GetOnlinedClients(new Empty());
        }

        public void ShutdownMaster(ShutdownMasterRequest shutdownMasterRequest)
        {
            _systemClient.ShutdownMaster(shutdownMasterRequest);
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

        public async Task BatchSyncPlayer(List<PlayerSaveDto> data, bool saveDB = false)
        {
            var req = new SyncProto.BatchSyncPlayerRequest() { SaveDb = saveDB };
            req.List.AddRange(data);
            await InternalSession.SendAsync(ChannelSendCode.BatchSyncPlayer, req);
        }


        public async Task SyncPlayer(PlayerSaveDto data, SyncCharacterTrigger trigger = SyncCharacterTrigger.Unknown, bool saveDB = false)
        {
            await InternalSession.SendAsync(ChannelSendCode.SyncPlayer, new SyncPlayerRequest { Trigger = (int)trigger, Data = data, SaveDb = saveDB });
        }
        public async Task SendAddBuddyRequest(BuddyProto.AddBuddyRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.AddBuddy, request);
        }

        public async Task SendAddBuddyRequest(BuddyProto.AddBuddyByIdRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.AddBuddyById, request);
        }

        public async Task SendBuddyMessage(BuddyProto.SendBuddyNoticeMessageDto request)
        {
            await InternalSession.SendAsync(ChannelSendCode.DropBuddyMessage, request);
        }

        public async Task SendDeleteBuddy(BuddyProto.DeleteBuddyRequest deleteBuddyRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.RemoveBuddy, deleteBuddyRequest);
        }

        public async Task SendWhisper(SendWhisperMessageRequest sendWhisperMessageRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.SendWhisper, sendWhisperMessageRequest);
        }

        public async Task GetLocation(BuddyProto.GetLocationRequest getLocationRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.GetLocation, getLocationRequest);
        }

        public UseCdkResponse UseCdk(UseCdkRequest useCdkRequest)
        {
            return _gameClient.UseCDK(useCdkRequest);
        }

        public void HealthCheck(MonitorData data)
        {
            _systemClient.HealthCheck(data);
        }

        public bool GainCharacterSlot(int accountId)
        {
            return _systemClient.GainCharacterSlot(new GainAccountCharacterSlotRequest { AccId = accountId }).Code == 0;
        }

        public void SendGuildPacket(GuildPacketRequest guildPacketRequest)
        {
            _guildClient.SendGuildPacket(guildPacketRequest);
        }

        public async Task SendMultiChatAsync(int type, string fromName, string msg, int[] receivers)
        {
            var data = new MessageProto.MultiChatMessage { Type = type, FromName = fromName, Text = msg };
            data.Receivers.AddRange(receivers);
            await InternalSession.SendAsync(ChannelSendCode.MultiChat, data);
        }

        public async Task SaveAllNotifyAsync()
        {
            await InternalSession.SendAsync(ChannelSendCode.SaveAll);
        }

        public async Task DisconnectAllNotifyAsync()
        {
            await InternalSession.SendAsync(ChannelSendCode.DisconnectAll);
        }

        public async Task CreateDueyPackage(CreatePackageRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.CreateDueyPackage);
        }

        public async Task TakeDueyPackage(TakeDueyPackageRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.TakeDueyPackage);
        }

        public async Task RequestRemovePackage(RemovePackageRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.RemoveDueyPackage);
        }

        public async Task GetDueyPackagesByPlayerId(GetPlayerDueyPackageRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.LoadDueyPackage);
        }

        public async Task TakeDueyPackageCommit(TakeDueyPackageCommit takeDueyPackageCommit)
        {
            await InternalSession.SendAsync(ChannelSendCode.TakeDueyPackageCallback);
        }
    }
}
