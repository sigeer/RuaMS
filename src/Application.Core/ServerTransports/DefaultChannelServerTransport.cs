using Application.Core.Channel;
using Application.Core.Channel.Internal;
using Application.Shared.Events;
using Application.Shared.Login;
using Application.Shared.Message;
using Application.Shared.Servers;
using Application.Shared.Team;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;

namespace Application.Core.ServerTransports
{
    public class DefaultChannelServerTransport : IChannelServerTransport
    {
        readonly ProtoService.SystemService.SystemServiceClient _systemClient;
        readonly ProtoService.GameService.GameServiceClient _gameClient;
        readonly ProtoService.SyncService.SyncServiceClient _syncClient;
        readonly ProtoService.GuildService.GuildServiceClient _guildClient;
        readonly ProtoService.AllianceService.AllianceServiceClient _allianceClient;
        readonly ProtoService.DataService.DataServiceClient _dataClient;
        readonly ProtoService.ItemService.ItemServiceClient _itemClient;
        readonly ProtoService.CashService.CashServiceClient _cashClient;
        readonly ProtoService.TeamService.TeamServiceClient _teamClient;
        readonly ProtoService.DueyService.DueyServiceClient _dueyClient;

        Lazy<InternalSession> _internalSession;
        public InternalSession InternalSession => _internalSession.Value;

        readonly ChannelServerConfig _config;
        IServiceProvider _sp;
        public DefaultChannelServerTransport(
            IServiceProvider sp,
            IOptions<ChannelServerConfig> options,
            ProtoService.SystemService.SystemServiceClient systemClient,
            ProtoService.GameService.GameServiceClient gameClient,
            ProtoService.SyncService.SyncServiceClient syncClient,
            ProtoService.GuildService.GuildServiceClient guildClient,
            ProtoService.AllianceService.AllianceServiceClient allianceClient,
            ProtoService.DataService.DataServiceClient dataClient,
            ProtoService.ItemService.ItemServiceClient itemClient,
            ProtoService.CashService.CashServiceClient cashClient,
            ProtoService.TeamService.TeamServiceClient teamClient,
            ProtoService.DueyService.DueyServiceClient dueyClient)
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

            _internalSession = new Lazy<InternalSession>(() => new InternalSession(_sp.GetRequiredService<WorldChannelServer>()));
            _dueyClient = dueyClient;
        }

        public async Task SendAsync(int type, CancellationToken cancellationToken = default)
        {
            await InternalSession.SendAsync(type, cancellationToken);
        }
        public async Task SendAsync(int type, IMessage message, CancellationToken cancellationToken = default)
        {
            await InternalSession.SendAsync(type, message, cancellationToken);
        }

        public long GetCurrentTime()
        {
            return _systemClient.GetCurrentTime(new Google.Protobuf.WellKnownTypes.Empty()).Value;
        }

        public int GetCurrentTimestamp()
        {
            return (int)_systemClient.GetCurrentTimestamp(new Google.Protobuf.WellKnownTypes.Empty()).Value;
        }

        public async Task BroadcastMessage(ProtoService.PacketRequest p)
        {
            await InternalSession.SendAsync(ChannelSendCode.BroadcastPacket, p);
        }

        public async Task SendWorldConfig(ProtoModel.WorldConfig updatePatch)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateWorldConfig, updatePatch);
        }

        public IPEndPoint GetChannelEndPoint(int channel)
        {
            var res = _systemClient.GetChannelEndPoint(new ProtoService.GetChannelEndPointRequest { Channel = channel });
            return new IPEndPoint(new IPAddress(res.Address.ToByteArray()), res.Port);
        }

        public ProtoModel.PlayerGetterProto GetPlayerData(string clientSession, int cid)
        {
            return _syncClient.GetPlayerObject(new ProtoService.GetPlayerByLoginRequest { ClientSession = clientSession, CharacterId = cid }).Data;
        }

        public async Task SendTimer(int seconds)
        {
            await InternalSession.SendAsync(ChannelSendCode.SetTimer, new ProtoModel.SetTimer { Seconds = seconds });
        }
        public async Task RemoveTimer()
        {
            await InternalSession.SendAsync(ChannelSendCode.RemoveTimer);
        }

        public async Task RegisterServer(List<ChannelConfig> channels, CancellationToken cancellationToken)
        {
            var streamingCall = _systemClient.Connect();
            InternalSession.Connect(streamingCall);

            var req = new ProtoService.RegisterServerRequest { ServerName = _config.ServerName, ServerHost = _config.ServerHost };
            req.Channels.AddRange(channels.Select(x => new ProtoModel.RegisterChannelConfigProto { Port = x.Port, MaxSize = x.MaxSize }));

            await InternalSession.SendAsync(ChannelSendCode.RegisterChannel, req, cancellationToken);
        }

        public async Task CompleteChannelShutdown()
        {
            await InternalSession.DisconnectAsync();
        }

        public async Task DropWorldMessage(ProtoService.DropMessageRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.DropMessage, request);
        }

        public void RemoveGuildQueued(int guildId)
        {
            _dataClient.RemoveGuildQueued(new ProtoService.GuildQueueRequest { GuildId = guildId });
        }

        public bool IsGuildQueued(int guildId)
        {
            return _dataClient.IsGuildQueued(new ProtoService.GuildQueueRequest { GuildId = guildId }).Value;
        }

        public void PutGuildQueued(int guildId)
        {
            _dataClient.PutGuildQueued(new ProtoService.GuildQueueRequest { GuildId = guildId });
        }

        public async Task CreateTeam(ProtoService.CreateTeamRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.CreateTeam, request);
        }

        public void SendAccountLogout(int accountId)
        {
            _syncClient.UpdateAccountState(new ProtoService.UpdateAccountStateRequest { AccId = accountId, State = LoginStage.LOGIN_NOTLOGGEDIN });
        }

        public AccountLoginStatus UpdateAccountState(int accId, sbyte state)
        {
            var res = _syncClient.UpdateAccountState(new ProtoService.UpdateAccountStateRequest { AccId = accId, State = state });
            return new AccountLoginStatus(res.State, res.Time.ToDateTimeOffset());
        }

        public void SetCharacteridInTransition(string v, int cid)
        {
            _syncClient.SetCharacterTransition(new ProtoService.SetClientCharacterTransitionRequest { CharacterId = cid, ClientSession = v });
        }

        public bool HasCharacteridInTransition(string clientSession)
        {
            return _syncClient.HasCharacterInTransition(new ProtoService.CheckCharacterInTransitionRequest { ClientSession = clientSession }).Value;
        }

        public bool CheckCharacterName(string name)
        {
            return _gameClient.CheckCharacterName(new ProtoService.CheckCharacterNameRequest { Name = name }).Value;
        }

        public void SendBuffObject(int v, ProtoModel.PlayerBuffProto playerBuffSaveDto)
        {
            _syncClient.PushPlayerBuffers(new ProtoService.PushPlayerBuffsRequest { CharacterId = v, Data = playerBuffSaveDto });
        }

        public ProtoModel.PlayerBuffProto GetBuffObject(int id)
        {
            return _syncClient.GetPlayerBuffers(new ProtoService.GetPlayerBufferRequest { CharacterId = id });
        }

        public async Task SetPlayerOnlined(int id, int channelId)
        {
            await InternalSession.SendAsync(ChannelSendCode.CompleteLogin, new ProtoService.CompleteLoginRequest { CharacterId = id, Channel = channelId });
        }

        public ProtoModel.DropAllProto RequestAllReactorDrops()
        {
            return _gameClient.LoadReactorDropData(new Empty());
        }

        public int[] RequestReactorSkillBooks()
        {
            return _gameClient.LoadReactorSkillBookData(new Empty()).IdList.ToArray();
        }

        public ProtoModel.SpecialCashItemListProto RequestSpecialCashItems()
        {
            return _cashClient.LoadSpecialItems(new Empty());
        }

        public ProtoService.GetMyGiftsResponse LoadPlayerGifts(ProtoService.GetMyGiftsRequest request)
        {
            return _cashClient.LoadGifts(request);
        }

        public void ClearGifts(int[] giftIdArray)
        {
            var req = new ProtoService.CommitRetrieveGiftRequest();
            req.IdList.AddRange(giftIdArray);
            _cashClient.CommitRetrieveGift(req);
        }

        public async Task<bool> SendNormalNoteMessage(int senderId, string toName, string noteMessage)
        {
            return (await _gameClient.SendNoteAsync(new ProtoService.SendNormalNoteRequest { FromId = senderId, Message = noteMessage, ToName = toName })).Value;
        }

        public ProtoModel.NoteProto? DeleteNoteMessage(int id)
        {
            return _gameClient.SetNoteRead(new ProtoService.SetNoteReadRequest { Id = id }).Data;
        }

        public ProtoModel.ShopProto? GetShop(int id, bool isShopId)
        {
            return _gameClient.GetShop(new ProtoService.GetShopRequest { Id = id, IsShopId = isShopId }).Data;
        }

        public ProtoService.LoadCharacterRankResponse LoadPlayerRanking(int topCount)
        {
            return _gameClient.LoadCharacterRank(new ProtoService.LoadCharacterRankRequest { Count = topCount });
        }

        public int[][] GetMostSellerCashItems()
        {
            return _cashClient.LoadMosterSellItems(new Empty()).Tabs.Select(x => x.ItemIdList.ToArray()).ToArray();
        }

        public ProtoService.OwlSearchRecordResponse GetOwlSearchedItems()
        {
            return _itemClient.LoadOwlSearchRecords(new Empty());
        }

        public ProtoService.OwlSearchResponse SendOwlSearch(ProtoService.OwlSearchRequest owlSearchRequest)
        {
            return _itemClient.UseOwlSearch(owlSearchRequest);
        }
        public async Task SendUpdateTeam(int teamId, PartyOperation operation, int fromId, int toId, int reason)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateTeam,
                new ProtoService.UpdateTeamRequest
                {
                    FromId = fromId,
                    Operation = (int)operation,
                    TargetId = toId,
                    TeamId = teamId,
                    Reason = reason
                });
        }
        public ProtoService.GetTeamResponse GetTeam(int party)
        {
            return _teamClient.GetTeamModel(new ProtoService.GetTeamRequest { Id = party });
        }

        public ProtoService.GetGuildResponse GetGuild(int id)
        {
            return _guildClient.GetGuildModel(new ProtoService.GetGuildRequest { Id = id });
        }

        public async Task CreateGuild(ProtoService.CreateGuildRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.CreateGuild, request);
        }

        public async Task BroadcastGuildMessage(int guildId, int v, string callout)
        {
            await InternalSession.SendAsync(ChannelSendCode.DropGuildMessage, new ProtoService.GuildDropMessageRequest { GuildId = guildId, Type = v, Message = callout });
        }

        public async Task SendUpdateGuildGP(ProtoService.UpdateGuildGPRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateGuildGp, request);
        }

        public async Task SendUpdateGuildRankTitle(ProtoService.UpdateGuildRankTitleRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateGuildRankTitle, request);
        }

        public async Task SendUpdateGuildNotice(ProtoService.UpdateGuildNoticeRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateGuildNotice, request);
        }

        public async Task SendUpdateGuildCapacity(ProtoService.UpdateGuildCapacityRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateGuildCapacity, request);
        }

        public async Task SendUpdateGuildEmblem(ProtoService.UpdateGuildEmblemRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateGuildEmblem, request);
        }

        public async Task SendGuildDisband(ProtoService.GuildDisbandRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.DisbandGuild, request);
        }

        public async Task SendChangePlayerGuildRank(ProtoService.UpdateGuildMemberRankRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.ChangeGuildMemberRank, request);
        }

        public async Task SendGuildExpelMember(ProtoService.ExpelFromGuildRequest expelFromGuildRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.ExpelGuildMember, expelFromGuildRequest);
        }

        public async Task SendPlayerLeaveGuild(ProtoService.LeaveGuildRequest leaveGuildRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.LeaveGuild, leaveGuildRequest);
        }

        public async Task SendPlayerJoinGuild(ProtoService.JoinGuildRequest joinGuildRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.JoinGuild, joinGuildRequest);
        }

        public ProtoService.GetAllianceResponse GetAlliance(int id)
        {
            return _allianceClient.GetAllianceModel(new ProtoService.GetAllianceRequest { Id = id });
        }

        public ProtoService.CreateAllianceCheckResponse CreateAllianceCheck(ProtoService.CreateAllianceCheckRequest request)
        {
            return _allianceClient.CreateAllianceCheck(request);
        }

        public async Task CreateAlliance(ProtoService.CreateAllianceRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.CreateAlliance, request);
        }

        public async Task SendGuildLeaveAlliance(ProtoService.GuildLeaveAllianceRequest guildLeaveAllianceRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.LeaveAlliance, guildLeaveAllianceRequest);
        }

        public async Task SendAllianceExpelGuild(ProtoService.AllianceExpelGuildRequest allianceExpelGuildRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.ExpelAllianceGuild, allianceExpelGuildRequest);
        }

        public async Task SendChangeAllianceLeader(ProtoService.AllianceChangeLeaderRequest allianceChangeLeaderRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateAllianceLeader, allianceChangeLeaderRequest);
        }

        public async Task SendChangePlayerAllianceRank(ProtoService.ChangePlayerAllianceRankRequest changePlayerAllianceRankRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateAllianceGuildRank, changePlayerAllianceRankRequest);
        }

        public async Task SendIncreaseAllianceCapacity(ProtoService.IncreaseAllianceCapacityRequest increaseAllianceCapacityRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateAllianceCapacity, increaseAllianceCapacityRequest);
        }

        public async Task SendUpdateAllianceRankTitle(ProtoService.UpdateAllianceRankTitleRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateAllianceRankTitle, request);
        }

        public async Task SendUpdateAllianceNotice(ProtoService.UpdateAllianceNoticeRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.UpdateAllianceNotice, request);
        }

        public async Task SendAllianceDisband(ProtoService.DisbandAllianceRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.DisbandAlliance, request);
        }

        public async Task AllianceBroadcastPlayerInfo(ProtoService.AllianceBroadcastPlayerInfoRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.AllianceBroadcastPlayerInfo, request);
        }

        public async Task SendPlayerJoinChatRoom(ProtoService.JoinChatRoomRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.JoinChatRoom, request);
        }

        public async Task SendPlayerLeaveChatRoom(ProtoService.LeaveChatRoomRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.LeaveChatRoom, request);
        }

        public async Task SendChatRoomMesage(ProtoService.SendChatRoomMessageRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.SendChatRoomMessage, request);
        }

        public async Task SendCreateChatRoom(ProtoService.CreateChatRoomRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.CreateChatRoom, request);
        }

        public async Task SendInvitation(ProtoService.CreateInviteRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.SendInvitation, request);
        }

        public async Task AnswerInvitation(ProtoService.AnswerInviteRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.AnswerInvitation, request);
        }

        public void RegisterExpedition(ProtoModel.ExpeditionRegistry request)
        {
            _gameClient.RegisterExpedition(request);
        }

        public ProtoService.ExpeditionCheckResponse CanStartExpedition(ProtoService.ExpeditionCheckRequest expeditionCheckRequest)
        {
            return _gameClient.CheckExpedition(expeditionCheckRequest);
        }

        public async Task ReceiveNewYearCard(ProtoService.ReceiveNewYearCardRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.ReceiveNewYearCard, request);
        }

        public async Task SendNewYearCard(ProtoService.SendNewYearCardRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.SendNewYearCard, request);
        }

        public async Task SendDiscardNewYearCard(ProtoService.DiscardNewYearCardRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.DiscardNewYearCard, request);
        }

        public ProtoService.SetFlyResponse SendSetFly(ProtoService.SetFlyRequest setFlyRequest)
        {
            return _systemClient.SetAccountFly(setFlyRequest);
        }

        public async Task SendReloadEvents(ProtoService.ReloadEventsRequest reloadEventsRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.ReloadWorldEvents, reloadEventsRequest);
        }

        public ProtoService.CreateTVMessageResponse BroadcastTV(ProtoService.CreateTVMessageRequest request)
        {
            return _itemClient.UseTVMessage(request);
        }

        public ProtoService.UseItemMegaphoneResponse SendItemMegaphone(ProtoService.UseItemMegaphoneRequest request)
        {
            return _itemClient.UseMegaPhone(request);
        }

        public ProtoModel.DropAllProto RequestDropData()
        {
            return _gameClient.LoadMobDropData(new Empty());
        }

        public ProtoService.QueryMonsterCardDataResponse RequestMonsterCardData()
        {
            return _gameClient.LoadMonsterCardData(new Empty());
        }

        public ProtoService.QueryRankedGuildsResponse RequestRankedGuilds()
        {
            return _guildClient.GetGuildRank(new Empty());
        }

        public ProtoService.GetPLifeByMapIdResponse RequestPLifeByMapId(ProtoService.GetPLifeByMapIdRequest requestPLifeByMapIdRequest)
        {
            return _gameClient.GetLifeByMapId(requestPLifeByMapIdRequest);
        }

        public ProtoService.GetAllPLifeResponse GetAllPLife(ProtoService.GetAllPLifeRequest request)
        {
            return _gameClient.GetAllPLife(request);
        }

        public async Task SendCreatePLife(ProtoService.CreatePLifeRequest createPLifeRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.CreatePLife, createPLifeRequest);
        }

        public async Task SendRemovePLife(ProtoService.RemovePLifeRequest removePLifeRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.RemovePLife, removePLifeRequest);
        }

        public ProtoService.BuyCashItemResponse SendBuyCashItem(ProtoService.BuyCashItemRequest buyCashItemRequest)
        {
            return _cashClient.BuyCashItem(buyCashItemRequest);
        }

        public ProtoModel.RemoteHiredMerchantProto LoadPlayerHiredMerchant(ProtoService.GetPlayerHiredMerchantRequest getPlayerShopRequest)
        {
            return _gameClient.LoadPlayerHiredMerchant(getPlayerShopRequest);
        }

        public void SyncPlayerShop(ProtoService.SyncPlayerShopRequest request)
        {
            _syncClient.SyncPlayerShop(request);
        }

        public ProtoService.CommitRetrievedResponse CommitRetrievedFromFredrick(ProtoService.CommitRetrievedRequest commitRetrievedRequest)
        {
            return _gameClient.CommitRetrievedFromFredrick(commitRetrievedRequest);
        }

        public async Task BatchSyncPlayerShop(ProtoService.BatchSyncPlayerShopRequest request)
        {
            await _syncClient.BatchSyncPlayerShopAsync(request);
        }

        public ProtoService.StoreItemsResponse SaveItems(ProtoService.StoreItemsRequest request)
        {
            return _itemClient.SaveItems(request);
        }

        public ProtoService.LoadItemsFromStoreResponse LoadItemFromStore(ProtoService.LoadItemsFromStoreRequest loadItemsFromStoreRequest)
        {
            return _itemClient.LoadItemFromStore(loadItemsFromStoreRequest);
        }

        public async Task BatchSyncMap(List<ProtoModel.MapSyncProto> data)
        {
            var req = new ProtoModel.MapBatchSyncProto();
            req.List.AddRange(data);
            await InternalSession.SendAsync(ChannelSendCode.SyncMap, req);
        }

        public async Task SendReport(ProtoService.SendReportRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.SendReport, request);
        }

        public async Task SetMonitor(ProtoService.ToggleMonitorPlayerRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.SetMonitor, request);
        }

        public ProtoModel.MonitorDataWrapperProto LoadMonitor()
        {
            return _gameClient.LoadMonitor(new Empty());
        }

        public async Task SetAutoBanIgnored(ProtoService.ToggleAutoBanIgnoreRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.SetAutobanIgnore, request);
        }

        public ProtoModel.AutoBanIgnoredWrapperProto LoadAutobanIgnoreData()
        {
            return _systemClient.GetAutobanIgnores(new Empty());
        }

        public async Task AntiMacroNotify(ProtoModel.AntiMacroNotifyMessageProto message)
        {
            await InternalSession.SendAsync(ChannelSendCode.AntiMacroNotify, message);
        }

        public async Task Ban(ProtoService.BanRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.Ban, request);
        }

        public async Task Unban(ProtoService.UnbanRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.Unban, request);
        }

        public async Task SetGmLevel(ProtoService.SetGmLevelRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.SetGmLevel, request);
        }

        public ProtoService.ShowOnlinePlayerResponse GetOnlinedPlayers()
        {
            return _systemClient.GetOnlinedPlayers(new Empty());
        }

        public async Task WarpPlayerByName(ProtoService.WrapPlayerByNameRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.WarpPlayer, request);
        }

        public async Task SummonPlayerByName(ProtoService.SummonPlayerByNameRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.SummonPlayer, request);
        }

        public async Task DisconnectPlayerByName(ProtoService.DisconnectPlayerByNameRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.DisconnectOne, request);
        }

        public ProtoModel.GetAllClientInfo GetOnliendClientInfo()
        {
            return _systemClient.GetOnlinedClients(new Empty());
        }

        public async Task ShutdownMaster(ProtoService.ShutdownMasterRequest shutdownMasterRequest)
        {
            await _systemClient.ShutdownMasterAsync(shutdownMasterRequest);
        }

        public ProtoModel.ServerStateProto GetServerState()
        {
            return _systemClient.GetServerState(new Empty());
        }

        public ProtoModel.GacheponDataProto GetGachaponData()
        {
            return _gameClient.LoadGachaponData(new Empty());
        }

        public ProtoService.NameChangeResponse ReigsterNameChange(ProtoService.NameChangeRequest nameChangeRequest)
        {
            return _gameClient.ChangeName(nameChangeRequest);
        }

        public async Task BatchSyncPlayer(List<ProtoModel.PlayerSaveProto> data, bool saveDB = false)
        {
            var req = new ProtoService.BatchSyncPlayerRequest() { SaveDb = saveDB };
            req.List.AddRange(data);
            await InternalSession.SendAsync(ChannelSendCode.BatchSyncPlayer, req);
        }


        public async Task SyncPlayer(ProtoModel.PlayerSaveProto data, SyncCharacterTrigger trigger = SyncCharacterTrigger.Unknown, bool saveDB = false)
        {
            await InternalSession.SendAsync(ChannelSendCode.SyncPlayer, new ProtoService.SyncPlayerRequest { Trigger = (int)trigger, Data = data, SaveDb = saveDB });
        }
        public async Task SendAddBuddyRequest(ProtoService.AddBuddyRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.AddBuddy, request);
        }

        public async Task SendAddBuddyRequest(ProtoService.AddBuddyByIdRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.AddBuddyById, request);
        }

        public async Task SendBuddyMessage(ProtoModel.SendBuddyNoticeMessageProto request)
        {
            await InternalSession.SendAsync(ChannelSendCode.DropBuddyMessage, request);
        }

        public async Task SendDeleteBuddy(ProtoService.DeleteBuddyRequest deleteBuddyRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.RemoveBuddy, deleteBuddyRequest);
        }

        public async Task SendWhisper(ProtoService.SendWhisperMessageRequest sendWhisperMessageRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.SendWhisper, sendWhisperMessageRequest);
        }

        public async Task GetLocation(ProtoService.GetLocationRequest getLocationRequest)
        {
            await InternalSession.SendAsync(ChannelSendCode.GetLocation, getLocationRequest);
        }

        public ProtoService.UseCdkResponse UseCdk(ProtoService.UseCdkRequest useCdkRequest)
        {
            return _gameClient.UseCDK(useCdkRequest);
        }

        public void HealthCheck(ProtoModel.MonitorData data)
        {
            _systemClient.HealthCheck(data);
        }

        public bool GainCharacterSlot(int accountId)
        {
            return _systemClient.GainCharacterSlot(new ProtoService.GainAccountCharacterSlotRequest { AccId = accountId }).Code == 0;
        }

        public async Task SendGuildPacket(ProtoService.GuildPacketRequest guildPacketRequest)
        {
            await _guildClient.SendGuildPacketAsync(guildPacketRequest);
        }

        public async Task SendMultiChatAsync(int type, string fromName, string msg, int[] receivers)
        {
            var data = new ProtoModel.MultiChatMessage { Type = type, FromName = fromName, Text = msg };
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

        public async Task<ProtoService.CreatePackageResponse> CreateDueyPackage(ProtoService.CreatePackageRequest request)
        {
            return await _dueyClient.CreateDueyPackageAsync(request);
        }

        public async Task TakeDueyPackage(ProtoService.TakeDueyPackageRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.TakeDueyPackage);
        }

        public async Task RequestRemovePackage(ProtoService.RemovePackageRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.RemoveDueyPackage);
        }

        public async Task GetDueyPackagesByPlayerId(ProtoService.GetPlayerDueyPackageRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.LoadDueyPackage);
        }

        public async Task TakeDueyPackageCommit(ProtoService.TakeDueyPackageCommitRequest takeDueyPackageCommit)
        {
            await InternalSession.SendAsync(ChannelSendCode.TakeDueyPackageCallback);
        }

        public async Task JailPlayer(ProtoService.CreateJailRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.Jail, request);
        }

        public async Task UnjailPlayer(ProtoService.CreateUnjailRequest request)
        {
            await InternalSession.SendAsync(ChannelSendCode.Unjail, request);
        }

        public async Task SendRemoveDoor(int ownerId)
        {
            await InternalSession.SendAsync(ChannelSendCode.RemoveDoor);
        }

        public async Task<ProtoModel.GetRewardsResponse> GetActiveRewards(ProtoModel.GetRewardsRequest request)
        {
            return await _gameClient.GetActiveRewardsAsync(request);
        }

        public async Task<ProtoService.UseCdkResponse> TakeReward(ProtoService.UseIdRequest request)
        {
            return await _gameClient.TakeRewardAsync(request);
        }
    }
}
