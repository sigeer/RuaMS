using Application.Core.Channel.Internal;
using Application.Core.Login;
using Application.Core.Login.ServerData;
using Application.Core.Login.ServerData.ExpeditionBossLog;
using Application.Core.Login.Services;
using Application.Core.ServerTransports;
using Application.Shared.Events;
using Application.Shared.Login;
using Application.Shared.Message;
using Application.Shared.Servers;
using Application.Shared.Team;
using Google.Protobuf;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Threading.Tasks;

namespace Application.Core.Channel.InProgress
{
    /// <summary>
    /// 登录服务器 与 频道服务器在同一个进程中时，直接与MasterServer交互
    /// </summary>
    public class LocalChannelServerTransport : IChannelServerTransport
    {
        readonly LoginService _loginService;
        readonly MasterServer _server;
        readonly ItemService _itemService;
        readonly NoteManager _noteService;
        readonly ShopManager _shopManager;
        readonly ReportService _msgService;
        readonly RankService _rankService;
        readonly InvitationService _invitationService;
        readonly ExpeditionManager _expeditionService;
        readonly PLifeDataManager _resourceService;
        readonly IMapper _mapper;
        readonly IServiceProvider _sp;
        public LocalChannelServerTransport(
            IServiceProvider sp,
            MasterServer server,
            LoginService loginService,
            ItemService itemService,
            NoteManager noteService,
            ShopManager shopManager,
            ReportService messageService,
            RankService rankService,
            InvitationService invitationService,
            ExpeditionManager expeditionService,
            PLifeDataManager resourceDataService,
            IMapper mapper)
        {
            _sp = sp;
            _server = server;
            _loginService = loginService;
            _itemService = itemService;
            _noteService = noteService;
            _shopManager = shopManager;
            _msgService = messageService;
            _mapper = mapper;
            _rankService = rankService;
            _invitationService = invitationService;
            _expeditionService = expeditionService;
            _resourceService = resourceDataService;
        }

        public Task SendAsync(int type, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        public Task SendAsync(int type, IMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public async Task RegisterServer(List<ChannelConfig> channels, CancellationToken cancellationToken = default)
        {
            var channelServer = _server.ServiceProvider.GetRequiredService<WorldChannelServer>();
            var serverNode = new InProgressNodeServer(channelServer, channels);
            if (!_server.IsRunning)
            {
                await channelServer.HandleServerRegistered(new ProtoModel.RegisterServerResultProto
                {
                    StartChannel = -1,
                    Message = "中心服务器未启动"
                }, cancellationToken);
            }
            else
            {
                var channelId = _server.AddChannel(serverNode);
                await channelServer.HandleServerRegistered(new ProtoModel.RegisterServerResultProto
                {
                    StartChannel = channelId,
                    Config = _server.GetWorldConfig()
                }, cancellationToken);
            }
        }

        public Task CompleteChannelShutdown()
        {
            _server.OnChannelShutdown(_sp.GetRequiredService<WorldChannelServer>().NodeConfig.ServerName);
            return Task.CompletedTask;
        }

        public async Task DropWorldMessage(ProtoService.DropMessageRequest request)
        {
            await _server.DropWorldMessage(request.Type, request.Message, request.OnlyGM);
        }

        public long GetCurrentTime()
        {
            return _server.getCurrentTime();
        }

        public int GetCurrentTimestamp()
        {
            return _server.getCurrentTimestamp();
        }

        public bool IsGuildQueued(int guildId)
        {
            return _server.IsGuildQueued(guildId);
        }

        public void PutGuildQueued(int guildId)
        {
            _server.PutGuildQueued(guildId);
        }
        public void RemoveGuildQueued(int guildId)
        {
            _server.RemoveGuildQueued(guildId);
        }

        public async Task SendWorldConfig(ProtoModel.WorldConfig updatePatch)
        {
            await _server.UpdateWorldConfig(updatePatch);
        }

        public async Task BroadcastMessage(ProtoService.PacketRequest p)
        {
            await _server.BroadcastPacket(p);
        }

        public async Task SendTimer(int seconds)
        {
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleSetTimer, new ProtoModel.SetTimer { Seconds = seconds });
        }

        public async Task RemoveTimer()
        {
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleRemoveTimer);
        }


        public void SendAccountLogout(int accountId)
        {
            _server.UpdateAccountState(accountId, LoginStage.LOGIN_NOTLOGGEDIN);
        }

        public IPEndPoint GetChannelEndPoint(int channel)
        {
            return _server.GetChannelIPEndPoint(channel);
        }

        public async Task BatchSyncMap(List<ProtoModel.MapSyncProto> data)
        {
            await _server.CharacterManager.BatchUpdateMap(data);
        }

        public AccountLoginStatus UpdateAccountState(int accId, sbyte state)
        {
            return _server.UpdateAccountState(accId, state);
        }

        public void SetCharacteridInTransition(string v, int cid)
        {
            _server.SetCharacteridInTransition(v, cid);
        }

        public bool HasCharacteridInTransition(string clientSession)
        {
            return _server.HasCharacteridInTransition(clientSession);
        }

        public ProtoModel.PlayerGetterProto? GetPlayerData(string clientSession, int cid)
        {
            return _loginService.PlayerLogin(clientSession, cid);
        }

        public bool CheckCharacterName(string name)
        {
            return _server.CharacterManager.CheckCharacterName(name);
        }

        public void SendBuffObject(int v, ProtoModel.PlayerBuffProto playerBuffSaveDto)
        {
            _server.BuffManager.SaveBuff(v, playerBuffSaveDto);
        }
        public ProtoModel.PlayerBuffProto GetBuffObject(int id)
        {
            return _server.BuffManager.Get(id);
        }

        public async Task SetPlayerOnlined(int id, int v)
        {
            await _loginService.SetPlayerLogedIn(id, v);
        }

        public ProtoModel.DropAllProto RequestAllReactorDrops()
        {
            return _server.DropDataManager.LoadAllReactorDrops();
        }

        public int[] RequestReactorSkillBooks()
        {
            return _itemService.LoadReactorSkillBooks();
        }

        public ProtoModel.SpecialCashItemListProto RequestSpecialCashItems()
        {
            return _itemService.LoadSpecialCashItems();
        }


        public ProtoService.GetMyGiftsResponse LoadPlayerGifts(ProtoService.GetMyGiftsRequest request)
        {
            return _server.GiftManager.LoadGifts(request);
        }
        public void ClearGifts(int[] giftIdArray)
        {
            _server.GiftManager.CommitRetrieveGift(giftIdArray);
        }

        public async Task<bool> SendNormalNoteMessage(int senderId, string toName, string noteMessage)
        {
            return await _noteService.SendNormal(noteMessage, senderId, toName);
        }

        public ProtoModel.NoteProto? DeleteNoteMessage(int id)
        {
            return _noteService.SetRead(id);
        }

        public ProtoModel.ShopProto? GetShop(int id, bool isShopId)
        {
            return _shopManager.LoadFromDB(id, isShopId);
        }

        public async Task SendReport(ProtoService.SendReportRequest request)
        {
            await _msgService.AddReport(request);
        }

        public ProtoService.LoadCharacterRankResponse LoadPlayerRanking(int topCount)
        {
            return _rankService.LoadPlayerRanking(topCount);
        }

        public int[][] GetMostSellerCashItems()
        {
            return _mapper.Map<int[][]>(_server.CashShopDataManager.GetMostSellerCashItems());
        }

        public ProtoService.OwlSearchRecordResponse GetOwlSearchedItems()
        {
            return _server.PlayerShopManager.GetOwlSearchedItems();
        }

        #region Team
        public async Task CreateTeam(ProtoService.CreateTeamRequest request)
        {
            var res =  _server.TeamManager.CreateTeam(request);
            await _server.Transport.SendMessageN(ChannelRecvCode.OnTeamCreated, res, [res.Request.LeaderId]);
        }
        public async Task SendUpdateTeam(int teamId, PartyOperation operation, int fromId, int toId, int reason)
        {
            await _server.TeamManager.UpdateParty(teamId, operation, fromId, toId, reason);
        }

        public ProtoService.GetTeamResponse GetTeam(int party)
        {
            return new ProtoService.GetTeamResponse() { Model = _server.TeamManager.GetTeamDto(party) };
        }

        #endregion

        #region Guild & Alliance
        public ProtoService.GetGuildResponse GetGuild(int id)
        {
            return new ProtoService.GetGuildResponse() { Model = _server.GuildManager.GetGuildFull(id) };
        }

        public async Task CreateGuild(ProtoService.CreateGuildRequest request)
        {
             await _server.GuildManager.CreateGuild(request);
        }

        public ProtoService.CreateAllianceCheckResponse CreateAllianceCheck(ProtoService.CreateAllianceCheckRequest request)
        {
            return _server.GuildManager.CreateAllianceCheck(request);
        }
        public async Task CreateAlliance(ProtoService.CreateAllianceRequest request)
        {
            await _server.GuildManager.CreateAlliance(request);
        }

        public ProtoService.GetAllianceResponse GetAlliance(int id)
        {
            return new ProtoService.GetAllianceResponse { Model = _server.GuildManager.GetAllianceDto(id) };
        }

        public async Task BroadcastGuildMessage(int guildId, int v, string callout)
        {
            await _server.GuildManager.SendGuildMessage(guildId, v, callout);
        }

        public async Task SendUpdateGuildGP(ProtoService.UpdateGuildGPRequest request)
        {
            await _server.GuildManager.UpdateGuildGPAsync(request);
        }

        public async Task SendUpdateGuildRankTitle(ProtoService.UpdateGuildRankTitleRequest request)
        {
            await _server.GuildManager.UpdateGuildRankTitle(request);
        }

        public async Task SendUpdateGuildNotice(ProtoService.UpdateGuildNoticeRequest request)
        {
            await _server.GuildManager.UpdateGuildNotice(request);
        }

        public async Task SendUpdateGuildCapacity(ProtoService.UpdateGuildCapacityRequest request)
        {
            await _server.GuildManager.IncreseGuildCapacity(request);
        }

        public async Task SendUpdateGuildEmblem(ProtoService.UpdateGuildEmblemRequest request)
        {
            await _server.GuildManager.UpdateGuildEmblem(request);
        }

        public async Task SendGuildDisband(ProtoService.GuildDisbandRequest request)
        {
            await _server.GuildManager.DisbandGuild(request);
        }

        public async Task SendChangePlayerGuildRank(ProtoService.UpdateGuildMemberRankRequest request)
        {
            await _server.GuildManager.ChangePlayerGuildRank(request);
        }

        public async Task SendGuildExpelMember(ProtoService.ExpelFromGuildRequest request)
        {
            await _server.GuildManager.GuildExpelMember(request);
        }

        public async Task SendPlayerLeaveGuild(ProtoService.LeaveGuildRequest request)
        {
            await _server.GuildManager.PlayerLeaveGuild(request);
        }

        public async Task SendPlayerJoinGuild(ProtoService.JoinGuildRequest request)
        {
            await _server.GuildManager.PlayerJoinGuild(request);
        }


        public async Task SendGuildLeaveAlliance(ProtoService.GuildLeaveAllianceRequest request)
        {
            await _server.GuildManager.GuildLeaveAlliance(request);
        }

        public async Task SendAllianceExpelGuild(ProtoService.AllianceExpelGuildRequest request)
        {
            await _server.GuildManager.AllianceExpelGuild(request);
        }

        public async Task SendChangeAllianceLeader(ProtoService.AllianceChangeLeaderRequest request)
        {
            await _server.GuildManager.ChangeAllianceLeader(request);
        }

        public async Task SendChangePlayerAllianceRank(ProtoService.ChangePlayerAllianceRankRequest request)
        {
            await _server.GuildManager.ChangePlayerAllianceRank(request);
        }

        public async Task SendIncreaseAllianceCapacity(ProtoService.IncreaseAllianceCapacityRequest request)
        {
            await _server.GuildManager.IncreaseAllianceCapacity(request);
        }

        public async Task SendUpdateAllianceRankTitle(ProtoService.UpdateAllianceRankTitleRequest request)
        {
            await _server.GuildManager.UpdateAllianceRankTitle(request);
        }

        public async Task SendUpdateAllianceNotice(ProtoService.UpdateAllianceNoticeRequest request)
        {
            await _server.GuildManager.UpdateAllianceNotice(request);
        }

        public async Task SendAllianceDisband(ProtoService.DisbandAllianceRequest request)
        {
            await _server.GuildManager.DisbandAlliance(request);
        }

        public async Task AllianceBroadcastPlayerInfo(ProtoService.AllianceBroadcastPlayerInfoRequest request)
        {
            await _server.GuildManager.AllianceBroadcastPlayerInfo(request);
        }
        #endregion

        #region ChatRoom
        public async Task SendPlayerJoinChatRoom(ProtoService.JoinChatRoomRequest request)
        {
            await _server.ChatRoomManager.JoinChatRoom(request);
        }

        public async Task SendPlayerLeaveChatRoom(ProtoService.LeaveChatRoomRequest request)
        {
            await _server.ChatRoomManager.LeaveChatRoom(request);
        }

        public async Task SendChatRoomMesage(ProtoService.SendChatRoomMessageRequest request)
        {
            await _server.ChatRoomManager.SendMessage(request);
        }

        public async Task SendCreateChatRoom(ProtoService.CreateChatRoomRequest request)
        {
            await _server.ChatRoomManager.CreateChatRoom(request);
        }
        #endregion

        public async Task SendInvitation(ProtoService.CreateInviteRequest request)
        {
            await _invitationService.AddInvitation(request);
        }

        public async Task AnswerInvitation(ProtoService.AnswerInviteRequest request)
        {
            await _invitationService.AnswerInvitation(request);
        }

        public void RegisterExpedition(ProtoModel.ExpeditionRegistry request)
        {
            _expeditionService.RegisterExpedition(request);
        }

        public ProtoService.ExpeditionCheckResponse CanStartExpedition(ProtoService.ExpeditionCheckRequest expeditionCheckRequest)
        {
            return _expeditionService.CanStartExpedition(expeditionCheckRequest);
        }


        public async Task ReceiveNewYearCard(ProtoService.ReceiveNewYearCardRequest request)
        {
            await _server.NewYearCardManager.ReceiveNewYearCard(request);
        }

        public async Task SendNewYearCard(ProtoService.SendNewYearCardRequest request)
        {
            await _server.NewYearCardManager.SendNewYearCard(request);
        }

        public async Task SendDiscardNewYearCard(ProtoService.DiscardNewYearCardRequest request)
        {
            await _server.NewYearCardManager.DiscardNewYearCard(request);
        }

        public ProtoService.SetFlyResponse SendSetFly(ProtoService.SetFlyRequest setFlyRequest)
        {
            return _server.AccountManager.SetFly(setFlyRequest);
        }

        public async Task SendReloadEvents(ProtoService.ReloadEventsRequest reloadEventsRequest)
        {
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleWorldEventReload, reloadEventsRequest);
        }

        public ProtoService.CreateTVMessageResponse BroadcastTV(ProtoService.CreateTVMessageRequest request)
        {
            return _itemService.BroadcastTV(request);
        }

        public ProtoService.UseItemMegaphoneResponse SendItemMegaphone(ProtoService.UseItemMegaphoneRequest request)
        {
            return _itemService.BroadcastItemMegaphone(request);
        }

        public ProtoModel.DropAllProto RequestDropData()
        {
            return _server.DropDataManager.LoadMobDropDto();
        }

        public ProtoService.QueryMonsterCardDataResponse RequestMonsterCardData()
        {
            return _itemService.LoadMonsterCard();
        }

        public ProtoService.QueryRankedGuildsResponse RequestRankedGuilds()
        {
            return _server.GuildManager.LoadRankedGuilds();
        }

        public ProtoService.GetPLifeByMapIdResponse RequestPLifeByMapId(ProtoService.GetPLifeByMapIdRequest request)
        {
            return _resourceService.LoadMapPLife(request);
        }

        public ProtoService.GetAllPLifeResponse GetAllPLife(ProtoService.GetAllPLifeRequest request)
        {
            return _resourceService.GetAllPLife();
        }

        public async Task SendCreatePLife(ProtoService.CreatePLifeRequest createPLifeRequest)
        {
            await _resourceService.CreatePLife(createPLifeRequest);
        }

        public async Task SendRemovePLife(ProtoService.RemovePLifeRequest removePLifeRequest)
        {
            await _resourceService.RemovePLife(removePLifeRequest);
        }

        public ProtoService.BuyCashItemResponse SendBuyCashItem(ProtoService.BuyCashItemRequest buyCashItemRequest)
        {
            return _server.CashShopDataManager.BuyCashItem(buyCashItemRequest);
        }

        public ProtoModel.RemoteHiredMerchantProto LoadPlayerHiredMerchant(ProtoService.GetPlayerHiredMerchantRequest getPlayerShopRequest)
        {
            return _server.PlayerShopManager.GetPlayerHiredMerchant(getPlayerShopRequest);
        }

        public void SyncPlayerShop(ProtoService.SyncPlayerShopRequest request)
        {
            _server.PlayerShopManager.SyncPlayerStorage(request);
        }

        public ProtoService.CommitRetrievedResponse CommitRetrievedFromFredrick(ProtoService.CommitRetrievedRequest commitRetrievedRequest)
        {
            return _server.PlayerShopManager.CommitRetrieve(commitRetrievedRequest);
        }


        public Task BatchSyncPlayerShop(ProtoService.BatchSyncPlayerShopRequest request)
        {
            foreach (var item in request.List)
            {
                _server.PlayerShopManager.SyncPlayerStorage(item);
            }
            return Task.CompletedTask;
        }
        public ProtoService.OwlSearchResponse SendOwlSearch(ProtoService.OwlSearchRequest request)
        {
            return _server.PlayerShopManager.OwlSearch(request);
        }

        public async Task SetMonitor(ProtoService.ToggleMonitorPlayerRequest toggleMonitorPlayerRequest)
        {
            await _server.SystemManager.ToggleMonitor(toggleMonitorPlayerRequest);
        }

        public ProtoModel.MonitorDataWrapperProto LoadMonitor()
        {
            return _server.SystemManager.LoadMonitorData();
        }

        public async Task SetAutoBanIgnored(ProtoService.ToggleAutoBanIgnoreRequest toggleAutoBanIgnoreRequest)
        {
            await _server.SystemManager.ToggleAutoBanIgnored(toggleAutoBanIgnoreRequest);
        }

        public ProtoModel.AutoBanIgnoredWrapperProto LoadAutobanIgnoreData()
        {
            return _server.SystemManager.LoadAutobanIgnoreData();
        }

        public async Task Ban(ProtoService.BanRequest banRequest)
        {
            await _server.AccountBanManager.Ban(banRequest);
        }

        public async Task Unban(ProtoService.UnbanRequest unbanRequest)
        {
            await _server.AccountBanManager.Unban(unbanRequest);
        }

        public async Task SetGmLevel(ProtoService.SetGmLevelRequest setGmLevelRequest)
        {
            await _server.AccountManager.SetGmLevel(setGmLevelRequest);
        }

        public ProtoService.ShowOnlinePlayerResponse GetOnlinedPlayers()
        {
            return _server.CharacterManager.GetOnlinedPlayers();
        }

        public async Task WarpPlayerByName(ProtoService.WrapPlayerByNameRequest wrapPlayerByNameRequest)
        {
            await _server.CrossServerService.WarpPlayerByName(wrapPlayerByNameRequest);
        }

        public async Task SummonPlayerByName(ProtoService.SummonPlayerByNameRequest summonPlayerByNameRequest)
        {
            await _server.CrossServerService.SummonPlayerByName(summonPlayerByNameRequest);
        }

        public async Task DisconnectPlayerByName(ProtoService.DisconnectPlayerByNameRequest request)
        {
            await _server.CrossServerService.DisconnectPlayerByName(request);
        }

        public ProtoModel.GetAllClientInfo GetOnliendClientInfo()
        {
            return _server.AccountManager.GetOnliendClientInfo();
        }

        public ProtoModel.GacheponDataProto GetGachaponData()
        {
            return _server.GachaponManager.GetGachaponData();
        }

        public ProtoService.NameChangeResponse ReigsterNameChange(ProtoService.NameChangeRequest nameChangeRequest)
        {
            return _server.CharacterManager.ChangeName(nameChangeRequest);
        }

        public Task BatchSyncPlayer(List<ProtoModel.PlayerSaveProto> data, bool saveDB = false)
        {
            return _server.CharacterManager.BatchUpdateOrSave(data, saveDB);
        }

        public Task SyncPlayer(ProtoModel.PlayerSaveProto data, SyncCharacterTrigger trigger = SyncCharacterTrigger.Unknown, bool saveDB = false)
        {
            return _server.CharacterManager.UpdateOrSave(data, trigger, saveDB);
        }

        public async Task SendAddBuddyRequest(ProtoService.AddBuddyRequest request)
        {
            await _server.BuddyManager.AddBuddyByName(request);
        }

        public async Task SendAddBuddyRequest(ProtoService.AddBuddyByIdRequest request)
        {
            await _server.BuddyManager.AddBuddyById(request);
        }


        public async Task SendBuddyMessage(ProtoModel.SendBuddyNoticeMessageProto request)
        {
            await _server.BuddyManager.BroadcastNoticeMessage(request);
        }

        public async Task SendDeleteBuddy(ProtoService.DeleteBuddyRequest request)
        {
            await _server.BuddyManager.DeleteBuddy(request);
        }

        public async Task SendWhisper(ProtoService.SendWhisperMessageRequest request)
        {
            await _server.BuddyManager.SendWhisper(request);
        }

        public async Task GetLocation(ProtoService.GetLocationRequest request)
        {
            await _server.BuddyManager.GetLocation(request);
        }


        public async Task ShutdownMaster(ProtoService.ShutdownMasterRequest request)
        {
            await _server.Shutdown(request.DelaySeconds);
            await _server.DropWorldMessage(0, $"服务器将在 {TimeSpan.FromSeconds(request.DelaySeconds).ToString()} 后停止。");
        }


        public ProtoService.UseCdkResponse UseCdk(ProtoService.UseCdkRequest useCdkRequest)
        {
            return _server.RewardManager.UseCdk(useCdkRequest);
        }

        public ProtoModel.ServerStateProto GetServerState()
        {
            return _server.GetServerStats();
        }

        public void HealthCheck(ProtoModel.MonitorData data)
        {
            _server.ChannelNodeList[_server.ServiceProvider.GetRequiredService<WorldChannelServer>().InstanceName].HealthCheck(data);
        }

        public bool GainCharacterSlot(int accountId)
        {
            return _server.AccountManager.GainCharacterSlot(accountId);
        }

        public async Task SendGuildPacket(ProtoService.GuildPacketRequest guildPacketRequest)
        {
            await _server.GuildManager.SendGuildPacket(guildPacketRequest);
        }

        public async Task SendMultiChatAsync(int type, string fromName, string msg, int[] receivers)
        {
            if (type == 0)
                await _server.BuddyManager.SendBuddyChatAsync(fromName, msg, receivers);
            else if (type == 1)
                await _server.TeamManager.SendTeamChatAsync(fromName, msg);
            else if (type == 2)
                await _server.GuildManager.SendGuildChatAsync(fromName, msg);
            else if (type == 3)
                await _server.GuildManager.SendAllianceChatAsync(fromName, msg);
        }

        public async Task SaveAllNotifyAsync()
        {
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.SaveAll);
        }

        public async Task DisconnectAllNotifyAsync()
        {
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.DisconnectAll);
        }

        public async Task<ProtoService.CreatePackageResponse> CreateDueyPackage(ProtoService.CreatePackageRequest request)
        {
            return await _server.DueyManager.CreateDueyPackage(request);
        }

        public async Task TakeDueyPackage(ProtoService.TakeDueyPackageRequest request)
        {
            await _server.DueyManager.TakeDueyPackage(request);
        }

        public async Task RequestRemovePackage(ProtoService.RemovePackageRequest request)
        {
            await _server.DueyManager.RemovePackage(request);
        }

        public async Task GetDueyPackagesByPlayerId(ProtoService.GetPlayerDueyPackageRequest request)
        {
            await _server.DueyManager.GetPlayerDueyPackages(request);
        }

        public async Task TakeDueyPackageCommit(ProtoService.TakeDueyPackageCommitRequest request)
        {
            await _server.DueyManager.TakeDueyPackageCommit(request);
        }

        public async Task JailPlayer(ProtoService.CreateJailRequest request)
        {
            await _server.CharacterManager.JailPlayer(request);
        }

        public async Task UnjailPlayer(ProtoService.CreateUnjailRequest request)
        {
            await _server.CharacterManager.UnjailPlayer(request);
        }

        public async Task SendRemoveDoor(int ownerId)
        {
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.OnDoorRemoved);
        }

        public async Task AntiMacroNotify(ProtoModel.AntiMacroNotifyMessageProto message)
        {
            await _server.ProcessAntiMacroPenalty(message);
        }

        public Task<ProtoModel.GetRewardsResponse> GetActiveRewards(ProtoModel.GetRewardsRequest request)
        {
            return _server.RewardManager.GetActiveRewards(request);
        }

        public Task<ProtoService.UseCdkResponse> TakeReward(ProtoService.UseIdRequest request)
        {
            return Task.FromResult(_server.RewardManager.UseId(request));
        }
    }
}
