using AllianceProto;
using Application.Core.Channel.Internal;
using Application.Core.Login;
using Application.Core.Login.ServerData;
using Application.Core.Login.Services;
using Application.Core.ServerTransports;
using Application.Shared.Events;
using Application.Shared.Login;
using Application.Shared.Message;
using Application.Shared.Servers;
using Application.Shared.Team;
using AutoMapper;
using BaseProto;
using CashProto;
using Config;
using CreatorProto;
using Dto;
using DueyDto;
using Google.Protobuf;
using GuildProto;
using ItemProto;
using JailProto;
using LifeProto;
using MessageProto;
using Microsoft.Extensions.DependencyInjection;
using ServerProto;
using SyncProto;
using System.Net;
using System.Threading.Tasks;
using SystemProto;
using TeamProto;

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
        readonly ShopService _shopManager;
        readonly MessageService _msgService;
        readonly RankService _rankService;
        readonly InvitationService _invitationService;
        readonly IExpeditionService _expeditionService;
        readonly ResourceDataManager _resourceService;
        readonly IMapper _mapper;
        readonly IServiceProvider _sp;
        public LocalChannelServerTransport(
            IServiceProvider sp,
            MasterServer server,
            LoginService loginService,
            ItemService itemService,
            NoteManager noteService,
            ShopService shopManager,
            MessageService messageService,
            RankService rankService,
            InvitationService invitationService,
            IExpeditionService expeditionService,
            ResourceDataManager resourceDataService,
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
            var serverNode = new InProgressWorldChannel(channelServer, channels);
            if (!_server.IsRunning)
            {
                await channelServer.HandleServerRegistered(new RegisterServerResult
                {
                    StartChannel = -1,
                    Message = "中心服务器未启动"
                }, cancellationToken);
            }
            else
            {
                var channelId = _server.AddChannel(serverNode);
                await channelServer.HandleServerRegistered(new RegisterServerResult
                {
                    StartChannel = channelId,
                    Coupon = _server.CouponManager.GetConfig(),
                    Config = _server.GetWorldConfig()
                }, cancellationToken);
            }
        }

        public async Task CreatePlayerResponseAsync(CreateCharResponseDto res)
        {
            _server.HandleCreateCharacterResponse(res);
        }

        public Task CompleteChannelShutdown()
        {
            _server.OnChannelShutdown(_sp.GetRequiredService<WorldChannelServer>().ServerName);
            return Task.CompletedTask;
        }

        public async Task DropWorldMessage(MessageProto.DropMessageRequest request)
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

        public async Task SendWorldConfig(Config.WorldConfig updatePatch)
        {
            await _server.UpdateWorldConfig(updatePatch);
        }

        public async Task BroadcastMessage(MessageProto.PacketRequest p)
        {
            await _server.BroadcastPacket(p);
        }

        public async Task SendTimer(int seconds)
        {
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleSetTimer, new MessageProto.SetTimer { Seconds = seconds });
        }

        public async Task RemoveTimer()
        {
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleRemoveTimer);
        }


        public SearchHiredMerchantChannelResponse FindPlayerShopChannel(SearchHiredMerchantChannelRequest request)
        {
            return _server.PlayerShopManager.FindHiredMerchantChannel(request);
        }

        public void SendAccountLogout(int accountId)
        {
            _server.UpdateAccountState(accountId, LoginStage.LOGIN_NOTLOGGEDIN);
        }

        public IPEndPoint GetChannelEndPoint(int channel)
        {
            return _server.GetChannelIPEndPoint(channel);
        }

        public async Task BatchSyncMap(List<SyncProto.MapSyncDto> data)
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

        public SyncProto.PlayerGetterDto? GetPlayerData(string clientSession, int cid)
        {
            return _loginService.PlayerLogin(clientSession, cid);
        }

        public bool CheckCharacterName(string name)
        {
            return _server.CharacterManager.CheckCharacterName(name);
        }

        public void SendBuffObject(int v, SyncProto.PlayerBuffDto playerBuffSaveDto)
        {
            _server.BuffManager.SaveBuff(v, playerBuffSaveDto);
        }
        public SyncProto.PlayerBuffDto GetBuffObject(int id)
        {
            return _server.BuffManager.Get(id);
        }

        public async Task SetPlayerOnlined(int id, int v)
        {
            await _loginService.SetPlayerLogedIn(id, v);
        }

        public Dto.DropAllDto RequestAllReactorDrops()
        {
            return _itemService.LoadAllReactorDrops();
        }

        public int[] RequestReactorSkillBooks()
        {
            return _itemService.LoadReactorSkillBooks();
        }

        public CashProto.SpecialCashItemListDto RequestSpecialCashItems()
        {
            return _itemService.LoadSpecialCashItems();
        }


        public GetMyGiftsResponse LoadPlayerGifts(GetMyGiftsRequest request)
        {
            return _server.GiftManager.LoadGifts(request);
        }
        public void ClearGifts(int[] giftIdArray)
        {
            _server.GiftManager.CommitRetrieveGift(giftIdArray);
        }

        public bool SendNormalNoteMessage(int senderId, string toName, string noteMessage)
        {
            return _noteService.SendNormal(noteMessage, senderId, toName).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public Dto.NoteDto? DeleteNoteMessage(int id)
        {
            return _noteService.SetRead(id);
        }

        public Dto.ShopDto? GetShop(int id, bool isShopId)
        {
            return _shopManager.LoadFromDB(id, isShopId);
        }

        public async Task SendReport(SendReportRequest request)
        {
            await _msgService.AddReport(request);
        }

        public RankProto.LoadCharacterRankResponse LoadPlayerRanking(int topCount)
        {
            return _rankService.LoadPlayerRanking(topCount);
        }

        public async Task SendToggleCoupon(int v)
        {
            await _server.CouponManager.ToggleCoupon(v);
        }
        public CreatorProto.CreateCharResponseDto SendNewPlayer(CreatorProto.NewPlayerSaveDto data)
        {
            return new CreatorProto.CreateCharResponseDto { Code = _server.CharacterManager.CreatePlayerDB(data) };
        }

        public CreatorProto.CreateCharCheckResponse CreatePlayerCheck(CreatorProto.CreateCharCheckRequest request)
        {
            return new CreatorProto.CreateCharCheckResponse()
            {
                Code = _server.CharacterManager.CreatePlayerCheck(request.AccountId, request.Name)
            };
        }

        public int[][] GetMostSellerCashItems()
        {
            return _mapper.Map<int[][]>(_server.CashShopDataManager.GetMostSellerCashItems());
        }

        public ItemProto.OwlSearchRecordResponse GetOwlSearchedItems()
        {
            return _server.PlayerShopManager.GetOwlSearchedItems();
        }

        #region Team
        public async Task CreateTeam(CreateTeamRequest request)
        {
            var res =  _server.TeamManager.CreateTeam(request);
            await _server.Transport.SendMessageN(ChannelRecvCode.OnTeamCreated, res, [res.Request.LeaderId]);
        }
        public async Task SendUpdateTeam(int teamId, PartyOperation operation, int fromId, int toId, int reason)
        {
            await _server.TeamManager.UpdateParty(teamId, operation, fromId, toId, reason);
        }

        public TeamProto.GetTeamResponse GetTeam(int party)
        {
            return new TeamProto.GetTeamResponse() { Model = _server.TeamManager.GetTeamDto(party) };
        }

        #endregion

        #region Guild & Alliance
        public GuildProto.GetGuildResponse GetGuild(int id)
        {
            return new GuildProto.GetGuildResponse() { Model = _server.GuildManager.GetGuildFull(id) };
        }

        public async Task CreateGuild(GuildProto.CreateGuildRequest request)
        {
             await _server.GuildManager.CreateGuild(request);
        }

        public AllianceProto.CreateAllianceCheckResponse CreateAllianceCheck(AllianceProto.CreateAllianceCheckRequest request)
        {
            return _server.GuildManager.CreateAllianceCheck(request);
        }
        public async Task CreateAlliance(AllianceProto.CreateAllianceRequest request)
        {
            await _server.GuildManager.CreateAlliance(request);
        }

        public AllianceProto.GetAllianceResponse GetAlliance(int id)
        {
            return new AllianceProto.GetAllianceResponse { Model = _server.GuildManager.GetAllianceDto(id) };
        }

        public async Task BroadcastGuildMessage(int guildId, int v, string callout)
        {
            await _server.GuildManager.SendGuildMessage(guildId, v, callout);
        }

        public async Task SendUpdateGuildGP(GuildProto.UpdateGuildGPRequest request)
        {
            await _server.GuildManager.UpdateGuildGPAsync(request);
        }

        public async Task SendUpdateGuildRankTitle(GuildProto.UpdateGuildRankTitleRequest request)
        {
            await _server.GuildManager.UpdateGuildRankTitle(request);
        }

        public async Task SendUpdateGuildNotice(GuildProto.UpdateGuildNoticeRequest request)
        {
            await _server.GuildManager.UpdateGuildNotice(request);
        }

        public async Task SendUpdateGuildCapacity(GuildProto.UpdateGuildCapacityRequest request)
        {
            await _server.GuildManager.IncreseGuildCapacity(request);
        }

        public async Task SendUpdateGuildEmblem(GuildProto.UpdateGuildEmblemRequest request)
        {
            await _server.GuildManager.UpdateGuildEmblem(request);
        }

        public async Task SendGuildDisband(GuildProto.GuildDisbandRequest request)
        {
            await _server.GuildManager.DisbandGuild(request);
        }

        public async Task SendChangePlayerGuildRank(GuildProto.UpdateGuildMemberRankRequest request)
        {
            await _server.GuildManager.ChangePlayerGuildRank(request);
        }

        public async Task SendGuildExpelMember(GuildProto.ExpelFromGuildRequest request)
        {
            await _server.GuildManager.GuildExpelMember(request);
        }

        public async Task SendPlayerLeaveGuild(GuildProto.LeaveGuildRequest request)
        {
            await _server.GuildManager.PlayerLeaveGuild(request);
        }

        public async Task SendPlayerJoinGuild(GuildProto.JoinGuildRequest request)
        {
            await _server.GuildManager.PlayerJoinGuild(request);
        }


        public async Task SendGuildLeaveAlliance(AllianceProto.GuildLeaveAllianceRequest request)
        {
            await _server.GuildManager.GuildLeaveAlliance(request);
        }

        public async Task SendAllianceExpelGuild(AllianceProto.AllianceExpelGuildRequest request)
        {
            await _server.GuildManager.AllianceExpelGuild(request);
        }

        public async Task SendChangeAllianceLeader(AllianceProto.AllianceChangeLeaderRequest request)
        {
            await _server.GuildManager.ChangeAllianceLeader(request);
        }

        public async Task SendChangePlayerAllianceRank(AllianceProto.ChangePlayerAllianceRankRequest request)
        {
            await _server.GuildManager.ChangePlayerAllianceRank(request);
        }

        public async Task SendIncreaseAllianceCapacity(AllianceProto.IncreaseAllianceCapacityRequest request)
        {
            await _server.GuildManager.IncreaseAllianceCapacity(request);
        }

        public async Task SendUpdateAllianceRankTitle(AllianceProto.UpdateAllianceRankTitleRequest request)
        {
            await _server.GuildManager.UpdateAllianceRankTitle(request);
        }

        public async Task SendUpdateAllianceNotice(AllianceProto.UpdateAllianceNoticeRequest request)
        {
            await _server.GuildManager.UpdateAllianceNotice(request);
        }

        public async Task SendAllianceDisband(AllianceProto.DisbandAllianceRequest request)
        {
            await _server.GuildManager.DisbandAlliance(request);
        }

        public async Task AllianceBroadcastPlayerInfo(AllianceBroadcastPlayerInfoRequest request)
        {
            await _server.GuildManager.AllianceBroadcastPlayerInfo(request);
        }
        #endregion

        #region ChatRoom
        public async Task SendPlayerJoinChatRoom(Dto.JoinChatRoomRequest request)
        {
            await _server.ChatRoomManager.JoinChatRoom(request);
        }

        public async Task SendPlayerLeaveChatRoom(Dto.LeaveChatRoomRequst request)
        {
            await _server.ChatRoomManager.LeaveChatRoom(request);
        }

        public async Task SendChatRoomMesage(Dto.SendChatRoomMessageRequest request)
        {
            await _server.ChatRoomManager.SendMessage(request);
        }

        public async Task SendCreateChatRoom(Dto.CreateChatRoomRequest request)
        {
            await _server.ChatRoomManager.CreateChatRoom(request);
        }
        #endregion

        public async Task SendInvitation(InvitationProto.CreateInviteRequest request)
        {
            await _invitationService.AddInvitation(request);
        }

        public async Task AnswerInvitation(InvitationProto.AnswerInviteRequest request)
        {
            await _invitationService.AnswerInvitation(request);
        }

        public void RegisterExpedition(ExpeditionProto.ExpeditionRegistry request)
        {
            _expeditionService.RegisterExpedition(request);
        }

        public ExpeditionProto.ExpeditionCheckResponse CanStartExpedition(ExpeditionProto.ExpeditionCheckRequest expeditionCheckRequest)
        {
            return _expeditionService.CanStartExpedition(expeditionCheckRequest);
        }


        public async Task ReceiveNewYearCard(Dto.ReceiveNewYearCardRequest request)
        {
            await _server.NewYearCardManager.ReceiveNewYearCard(request);
        }

        public async Task SendNewYearCard(Dto.SendNewYearCardRequest request)
        {
            await _server.NewYearCardManager.SendNewYearCard(request);
        }

        public async Task SendDiscardNewYearCard(Dto.DiscardNewYearCardRequest request)
        {
            await _server.NewYearCardManager.DiscardNewYearCard(request);
        }

        public ConfigProto.SetFlyResponse SendSetFly(ConfigProto.SetFlyRequest setFlyRequest)
        {
            return _server.AccountManager.SetFly(setFlyRequest);
        }

        public async Task SendReloadEvents(ReloadEventsRequest reloadEventsRequest)
        {
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleWorldEventReload, reloadEventsRequest);
        }

        public CreateTVMessageResponse BroadcastTV(CreateTVMessageRequest request)
        {
            return _itemService.BroadcastTV(request);
        }

        public UseItemMegaphoneResponse SendItemMegaphone(ItemProto.UseItemMegaphoneRequest request)
        {
            return _itemService.BroadcastItemMegaphone(request);
        }

        public DropAllDto RequestDropData()
        {
            return _itemService.LoadMobDropDto();
        }

        public QueryMonsterCardDataResponse RequestMonsterCardData()
        {
            return _itemService.LoadMonsterCard();
        }

        public GuildProto.QueryRankedGuildsResponse RequestRankedGuilds()
        {
            return _server.GuildManager.LoadRankedGuilds();
        }

        public LifeProto.GetPLifeByMapIdResponse RequestPLifeByMapId(LifeProto.GetPLifeByMapIdRequest request)
        {
            return _resourceService.LoadMapPLife(request);
        }

        public GetAllPLifeResponse GetAllPLife(GetAllPLifeRequest request)
        {
            return _resourceService.GetAllPLife();
        }

        public async Task SendCreatePLife(LifeProto.CreatePLifeRequest createPLifeRequest)
        {
            await _resourceService.CreatePLife(createPLifeRequest);
        }

        public async Task SendRemovePLife(LifeProto.RemovePLifeRequest removePLifeRequest)
        {
            await _resourceService.RemovePLife(removePLifeRequest);
        }

        public BuyCashItemResponse SendBuyCashItem(BuyCashItemRequest buyCashItemRequest)
        {
            return _server.CashShopDataManager.BuyCashItem(buyCashItemRequest);
        }

        public RemoteHiredMerchantDto LoadPlayerHiredMerchant(GetPlayerHiredMerchantRequest getPlayerShopRequest)
        {
            return _server.PlayerShopManager.GetPlayerHiredMerchant(getPlayerShopRequest);
        }

        public void SyncPlayerShop(SyncPlayerShopRequest request)
        {
            _server.PlayerShopManager.SyncPlayerStorage(request);
        }

        public CommitRetrievedResponse CommitRetrievedFromFredrick(CommitRetrievedRequest commitRetrievedRequest)
        {
            return _server.PlayerShopManager.CommitRetrieve(commitRetrievedRequest);
        }

        public CanHiredMerchantResponse CanHiredMerchant(CanHiredMerchantRequest canHiredMerchantRequest)
        {
            return _server.PlayerShopManager.CanHiredMerchant(canHiredMerchantRequest);
        }

        public Task BatchSyncPlayerShop(BatchSyncPlayerShopRequest request)
        {
            foreach (var item in request.List)
            {
                _server.PlayerShopManager.SyncPlayerStorage(item);
            }
            return Task.CompletedTask;
        }
        public ItemProto.OwlSearchResponse SendOwlSearch(OwlSearchRequest request)
        {
            return _server.PlayerShopManager.OwlSearch(request);
        }
        public StoreItemsResponse SaveItems(StoreItemsRequest request)
        {
            return _server.ItemFactoryManager.Store(request);
        }

        public LoadItemsFromStoreResponse LoadItemFromStore(LoadItemsFromStoreRequest request)
        {
            return _server.ItemFactoryManager.LoadItems(request);
        }

        public async Task SetMonitor(ToggleMonitorPlayerRequest toggleMonitorPlayerRequest)
        {
            await _server.SystemManager.ToggleMonitor(toggleMonitorPlayerRequest);
        }

        public MonitorDataWrapper LoadMonitor()
        {
            return _server.SystemManager.LoadMonitorData();
        }

        public async Task SetAutoBanIgnored(ToggleAutoBanIgnoreRequest toggleAutoBanIgnoreRequest)
        {
            await _server.SystemManager.ToggleAutoBanIgnored(toggleAutoBanIgnoreRequest);
        }

        public AutoBanIgnoredWrapper LoadAutobanIgnoreData()
        {
            return _server.SystemManager.LoadAutobanIgnoreData();
        }

        public async Task Ban(BanRequest banRequest)
        {
            await _server.AccountBanManager.Ban(banRequest);
        }

        public async Task Unban(UnbanRequest unbanRequest)
        {
            await _server.AccountBanManager.Unban(unbanRequest);
        }

        public async Task SetGmLevel(SetGmLevelRequest setGmLevelRequest)
        {
            await _server.AccountManager.SetGmLevel(setGmLevelRequest);
        }

        public ShowOnlinePlayerResponse GetOnlinedPlayers()
        {
            return _server.CharacterManager.GetOnlinedPlayers();
        }

        public async Task WarpPlayerByName(WrapPlayerByNameRequest wrapPlayerByNameRequest)
        {
            await _server.CrossServerService.WarpPlayerByName(wrapPlayerByNameRequest);
        }

        public async Task SummonPlayerByName(SummonPlayerByNameRequest summonPlayerByNameRequest)
        {
            await _server.CrossServerService.SummonPlayerByName(summonPlayerByNameRequest);
        }

        public async Task DisconnectPlayerByName(DisconnectPlayerByNameRequest request)
        {
            await _server.CrossServerService.DisconnectPlayerByName(request);
        }

        public GetAllClientInfo GetOnliendClientInfo()
        {
            return _server.AccountManager.GetOnliendClientInfo();
        }

        public GacheponDataDto GetGachaponData()
        {
            return _server.GachaponManager.GetGachaponData();
        }

        public NameChangeResponse ReigsterNameChange(NameChangeRequest nameChangeRequest)
        {
            return _server.CharacterManager.ChangeName(nameChangeRequest);
        }

        public Task BatchSyncPlayer(List<SyncProto.PlayerSaveDto> data, bool saveDB = false)
        {
            _server.CharacterManager.BatchUpdateOrSave(data, saveDB);
            return Task.CompletedTask;
        }

        public Task SyncPlayer(PlayerSaveDto data, SyncCharacterTrigger trigger = SyncCharacterTrigger.Unknown, bool saveDB = false)
        {
            _server.CharacterManager.UpdateOrSave(data, trigger, saveDB);
            return Task.CompletedTask;
        }

        public async Task SendAddBuddyRequest(BuddyProto.AddBuddyRequest request)
        {
            await _server.BuddyManager.AddBuddyByName(request);
        }

        public async Task SendAddBuddyRequest(BuddyProto.AddBuddyByIdRequest request)
        {
            await _server.BuddyManager.AddBuddyById(request);
        }


        public async Task SendBuddyMessage(BuddyProto.SendBuddyNoticeMessageDto request)
        {
            await _server.BuddyManager.BroadcastNoticeMessage(request);
        }

        public async Task SendDeleteBuddy(BuddyProto.DeleteBuddyRequest request)
        {
            await _server.BuddyManager.DeleteBuddy(request);
        }

        public async Task SendWhisper(SendWhisperMessageRequest request)
        {
            await _server.BuddyManager.SendWhisper(request);
        }

        public async Task GetLocation(BuddyProto.GetLocationRequest request)
        {
            await _server.BuddyManager.GetLocation(request);
        }


        public async Task ShutdownMaster(ShutdownMasterRequest request)
        {
            await _server.Shutdown(request.DelaySeconds);
            await _server.DropWorldMessage(0, $"服务器将在 {TimeSpan.FromSeconds(request.DelaySeconds).ToString()} 后停止。");
        }


        public UseCdkResponse UseCdk(UseCdkRequest useCdkRequest)
        {
            return _server.CDKManager.UseCdk(useCdkRequest);
        }

        public ServerStateDto GetServerState()
        {
            return _server.GetServerStats();
        }

        public void HealthCheck(MonitorData data)
        {
            _server.ChannelServerList[_server.ServiceProvider.GetRequiredService<WorldChannelServer>().ServerName].HealthCheck(data);
        }

        public bool GainCharacterSlot(int accountId)
        {
            return _server.AccountManager.GainCharacterSlot(accountId);
        }

        public async Task SendGuildPacket(GuildPacketRequest guildPacketRequest)
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

        public async Task<CreatePackageResponse> CreateDueyPackage(CreatePackageRequest request)
        {
            return await _server.DueyManager.CreateDueyPackage(request);
        }

        public async Task TakeDueyPackage(TakeDueyPackageRequest request)
        {
            await _server.DueyManager.TakeDueyPackage(request);
        }

        public async Task RequestRemovePackage(RemovePackageRequest request)
        {
            await _server.DueyManager.RemovePackage(request);
        }

        public async Task GetDueyPackagesByPlayerId(GetPlayerDueyPackageRequest request)
        {
            await _server.DueyManager.GetPlayerDueyPackages(request);
        }

        public async Task TakeDueyPackageCommit(TakeDueyPackageCommit request)
        {
            await _server.DueyManager.TakeDueyPackageCommit(request);
        }

        public async Task JailPlayer(CreateJailRequest request)
        {
            await _server.CharacterManager.JailPlayer(request);
        }

        public async Task UnjailPlayer(CreateUnjailRequest request)
        {
            await _server.CharacterManager.UnjailPlayer(request);
        }

        public async Task SendRemoveDoor(int ownerId)
        {
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.OnDoorRemoved);
        }
    }
}
