using Application.Core.Login;
using Application.Core.Login.ServerData;
using Application.Core.Login.Services;
using Application.Core.ServerTransports;
using Application.Resources;
using Application.Shared.Login;
using Application.Shared.MapObjects;
using Application.Shared.Message;
using Application.Shared.Models;
using Application.Shared.Team;
using AutoMapper;
using BaseProto;
using CashProto;
using Config;
using Dto;
using Google.Protobuf.WellKnownTypes;
using ItemProto;
using LifeProto;
using MessageProto;
using Microsoft.Extensions.DependencyInjection;
using ServerProto;
using SyncProto;
using System.Net;
using SystemProto;

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

        public LocalChannelServerTransport(
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

        public Task<Config.RegisterServerResult> RegisterServer(List<WorldChannel> channels)
        {
            if (!_server.IsRunning)
                return Task.FromResult(new Config.RegisterServerResult() { StartChannel = -1, Message = "中心服务器未启动" });

            var channelId = _server.AddChannel(new InProgressWorldChannel(_server.ServiceProvider.GetRequiredService<WorldChannelServer>(), channels));
            return Task.FromResult(new Config.RegisterServerResult
            {
                StartChannel = channelId,
                Coupon = _server.CouponManager.GetConfig(),
                Config = _server.GetWorldConfig()
            });
        }

        public void DropWorldMessage(MessageProto.DropMessageRequest request)
        {
            _server.DropWorldMessage(request.Type, request.Message, request.OnlyGM);
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

        public void SendWorldConfig(Config.WorldConfig updatePatch)
        {
            _server.UpdateWorldConfig(updatePatch);
        }

        public void BroadcastMessage(MessageProto.PacketRequest p)
        {
            _server.BroadcastPacket(p);
        }

        public void SendTimer(int seconds)
        {
            _server.Transport.BroadcastMessage(BroadcastType.Broadcast_SetTimer, new MessageProto.SetTimer { Seconds = seconds });
        }

        public void RemoveTimer()
        {
            _server.Transport.BroadcastMessage(BroadcastType.Broadcast_RemoveTimer, new MessageProto.RemoveTimer());
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

        public void BatchSyncMap(List<SyncProto.MapSyncDto> data)
        {
            _server.CharacterManager.BatchUpdateMap(data);
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

        public ExpeditionProto.QueryChannelExpedtionResponse GetExpeditionInfo()
        {
            return _server.Transport.QueryExpeditionInfo(new ExpeditionProto.QueryChannelExpedtionRequest());
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

        public void SetPlayerOnlined(int id, int v)
        {
            _loginService.SetPlayerLogedIn(id, v);
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
            return _noteService.SendNormal(noteMessage, senderId, toName);
        }

        public Dto.NoteDto? DeleteNoteMessage(int id)
        {
            return _noteService.SetRead(id);
        }

        public Dto.ShopDto? GetShop(int id, bool isShopId)
        {
            return _shopManager.LoadFromDB(id, isShopId);
        }

        public SendReportResponse SendReport(SendReportRequest request)
        {
            return _msgService.AddReport(request);
        }

        public RankProto.LoadCharacterRankResponse LoadPlayerRanking(int topCount)
        {
            return _rankService.LoadPlayerRanking(topCount);
        }

        public void SendToggleCoupon(int v)
        {
            _server.CouponManager.ToggleCoupon(v);
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
        public TeamProto.TeamDto CreateTeam(int playerId)
        {
            return _server.TeamManager.CreateTeam(playerId);
        }

        public TeamProto.UpdateTeamResponse SendUpdateTeam(int teamId, PartyOperation operation, int fromId, int toId)
        {
            return _server.TeamManager.UpdateParty(teamId, operation, fromId, toId);
        }

        public void SendTeamChat(string name, string chattext)
        {
            _server.TeamManager.SendTeamChat(name, chattext);
        }

        public TeamProto.GetTeamResponse GetTeam(int party)
        {
            return new TeamProto.GetTeamResponse() { Model = _server.TeamManager.GetTeamFull(party) };
        }




        #endregion

        #region Guild & Alliance
        public GuildProto.GetGuildResponse GetGuild(int id)
        {
            return new GuildProto.GetGuildResponse() { Model = _server.GuildManager.GetGuildFull(id) };
        }

        public GuildProto.GetGuildResponse CreateGuild(string guildName, int playerId, int[] members)
        {
            return new GuildProto.GetGuildResponse { Model = _server.GuildManager.CreateGuild(guildName, playerId, members) };
        }

        public AllianceProto.CreateAllianceCheckResponse CreateAllianceCheck(AllianceProto.CreateAllianceCheckRequest request)
        {
            return _server.GuildManager.CreateAllianceCheck(request);
        }
        public AllianceProto.GetAllianceResponse CreateAlliance(int[] masters, string allianceName)
        {
            return new AllianceProto.GetAllianceResponse { Model = _server.GuildManager.CreateAlliance(masters, allianceName) };
        }


        public AllianceProto.GetAllianceResponse GetAlliance(int id)
        {
            return new AllianceProto.GetAllianceResponse { Model = _server.GuildManager.GetAllianceFull(id) };
        }

        public void SendGuildChat(string name, string text)
        {
            _server.GuildManager.SendGuildChat(name, text);
        }

        public void SendAllianceChat(string name, string text)
        {
            _server.GuildManager.SendAllianceChat(name, text);
        }

        public void BroadcastGuildMessage(int guildId, int v, string callout)
        {
            _server.GuildManager.BroadcastGuildMessage(guildId, v, callout);
        }

        public void SendUpdateGuildGP(GuildProto.UpdateGuildGPRequest request)
        {
            _server.GuildManager.UpdateGuildGP(request);
        }

        public void SendUpdateGuildRankTitle(GuildProto.UpdateGuildRankTitleRequest request)
        {
            _server.GuildManager.UpdateGuildRankTitle(request);
        }

        public void SendUpdateGuildNotice(GuildProto.UpdateGuildNoticeRequest request)
        {
            _server.GuildManager.UpdateGuildNotice(request);
        }

        public void SendUpdateGuildCapacity(GuildProto.UpdateGuildCapacityRequest request)
        {
            _server.GuildManager.IncreseGuildCapacity(request);
        }

        public void SendUpdateGuildEmblem(GuildProto.UpdateGuildEmblemRequest request)
        {
            _server.GuildManager.UpdateGuildEmblem(request);
        }

        public void SendGuildDisband(GuildProto.GuildDisbandRequest request)
        {
            _server.GuildManager.DisbandGuild(request);
        }

        public void SendChangePlayerGuildRank(GuildProto.UpdateGuildMemberRankRequest request)
        {
            _server.GuildManager.ChangePlayerGuildRank(request);
        }

        public void SendGuildExpelMember(GuildProto.ExpelFromGuildRequest request)
        {
            _server.GuildManager.GuildExpelMember(request);
        }

        public void SendPlayerLeaveGuild(GuildProto.LeaveGuildRequest request)
        {
            _server.GuildManager.PlayerLeaveGuild(request);
        }

        public void SendPlayerJoinGuild(GuildProto.JoinGuildRequest request)
        {
            _server.GuildManager.PlayerJoinGuild(request);
        }

        public void SendGuildJoinAlliance(AllianceProto.GuildJoinAllianceRequest request)
        {
            _server.GuildManager.GuildJoinAlliance(request);
        }

        public void SendGuildLeaveAlliance(AllianceProto.GuildLeaveAllianceRequest request)
        {
            _server.GuildManager.GuildLeaveAlliance(request);
        }

        public void SendAllianceExpelGuild(AllianceProto.AllianceExpelGuildRequest request)
        {
            _server.GuildManager.AllianceExpelGuild(request);
        }

        public void SendChangeAllianceLeader(AllianceProto.AllianceChangeLeaderRequest request)
        {
            _server.GuildManager.ChangeAllianceLeader(request);
        }

        public void SendChangePlayerAllianceRank(AllianceProto.ChangePlayerAllianceRankRequest request)
        {
            _server.GuildManager.ChangePlayerAllianceRank(request);
        }

        public void SendIncreaseAllianceCapacity(AllianceProto.IncreaseAllianceCapacityRequest request)
        {
            _server.GuildManager.IncreaseAllianceCapacity(request);
        }

        public void SendUpdateAllianceRankTitle(AllianceProto.UpdateAllianceRankTitleRequest request)
        {
            _server.GuildManager.UpdateAllianceRankTitle(request);
        }

        public void SendUpdateAllianceNotice(AllianceProto.UpdateAllianceNoticeRequest request)
        {
            _server.GuildManager.UpdateAllianceNotice(request);
        }

        public void SendAllianceDisband(AllianceProto.DisbandAllianceRequest request)
        {
            _server.GuildManager.DisbandAlliance(request);
        }
        #endregion

        #region ChatRoom
        public void SendPlayerJoinChatRoom(Dto.JoinChatRoomRequest request)
        {
            _server.ChatRoomManager.JoinChatRoom(request);
        }

        public void SendPlayerLeaveChatRoom(Dto.LeaveChatRoomRequst request)
        {
            _server.ChatRoomManager.LeaveChatRoom(request);
        }

        public void SendChatRoomMesage(Dto.SendChatRoomMessageRequest request)
        {
            _server.ChatRoomManager.SendMessage(request);
        }

        public void SendCreateChatRoom(Dto.CreateChatRoomRequest request)
        {
            _server.ChatRoomManager.CreateChatRoom(request);
        }
        #endregion

        public void SendInvitation(InvitationProto.CreateInviteRequest request)
        {
            _invitationService.AddInvitation(request);
        }

        public void AnswerInvitation(InvitationProto.AnswerInviteRequest request)
        {
            _invitationService.AnswerInvitation(request);
        }

        public void RegisterExpedition(ExpeditionProto.ExpeditionRegistry request)
        {
            _expeditionService.RegisterExpedition(request);
        }

        public ExpeditionProto.ExpeditionCheckResponse CanStartExpedition(ExpeditionProto.ExpeditionCheckRequest expeditionCheckRequest)
        {
            return _expeditionService.CanStartExpedition(expeditionCheckRequest);
        }


        public void ReceiveNewYearCard(Dto.ReceiveNewYearCardRequest request)
        {
            _server.NewYearCardManager.ReceiveNewYearCard(request);
        }

        public void SendNewYearCard(Dto.SendNewYearCardRequest request)
        {
            _server.NewYearCardManager.SendNewYearCard(request);
        }

        public void SendDiscardNewYearCard(Dto.DiscardNewYearCardRequest request)
        {
            _server.NewYearCardManager.DiscardNewYearCard(request);
        }

        public ConfigProto.SetFlyResponse SendSetFly(ConfigProto.SetFlyRequest setFlyRequest)
        {
            return _server.AccountManager.SetFly(setFlyRequest);
        }

        public void SendReloadEvents(ReloadEventsRequest reloadEventsRequest)
        {
            _server.Transport.BroadcastMessage(BroadcastType.OnEventsReloaded, new ReloadEventsResponse { Code = 0, Request = reloadEventsRequest });
        }

        public ItemProto.CreateTVMessageResponse BroadcastTV(ItemProto.CreateTVMessageRequest request)
        {
            return _itemService.BroadcastTV(request);
        }

        public ItemProto.UseItemMegaphoneResponse SendItemMegaphone(ItemProto.UseItemMegaphoneRequest request)
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

        public void SendCreatePLife(LifeProto.CreatePLifeRequest createPLifeRequest)
        {
            _resourceService.CreatePLife(createPLifeRequest);
        }

        public void SendRemovePLife(LifeProto.RemovePLifeRequest removePLifeRequest)
        {
            _resourceService.RemovePLife(removePLifeRequest);
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

        public void BatchSyncPlayerShop(BatchSyncPlayerShopRequest request)
        {
            foreach (var item in request.List)
            {
                _server.PlayerShopManager.SyncPlayerStorage(item);
            }
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

        public ToggleMonitorPlayerResponse SetMonitor(ToggleMonitorPlayerRequest toggleMonitorPlayerRequest)
        {
            return _server.SystemManager.ToggleMonitor(toggleMonitorPlayerRequest);
        }

        public MonitorDataWrapper LoadMonitor()
        {
            return _server.SystemManager.LoadMonitorData();
        }

        public ToggleAutoBanIgnoreResponse SetAutoBanIgnored(ToggleAutoBanIgnoreRequest toggleAutoBanIgnoreRequest)
        {
            return _server.SystemManager.ToggleAutoBanIgnored(toggleAutoBanIgnoreRequest);
        }

        public AutoBanIgnoredWrapper LoadAutobanIgnoreData()
        {
            return _server.SystemManager.LoadAutobanIgnoreData();
        }

        public BanResponse Ban(BanRequest banRequest)
        {
            return _server.AccountBanManager.Ban(banRequest);
        }

        public UnbanResponse Unban(UnbanRequest unbanRequest)
        {
            return _server.AccountBanManager.Unban(unbanRequest);
        }

        public SetGmLevelResponse SetGmLevel(SetGmLevelRequest setGmLevelRequest)
        {
            return _server.AccountManager.SetGmLevel(setGmLevelRequest);
        }

        public ShowOnlinePlayerResponse GetOnlinedPlayers()
        {
            return _server.CharacterManager.GetOnlinedPlayers();
        }

        public WrapPlayerByNameResponse WarpPlayerByName(WrapPlayerByNameRequest wrapPlayerByNameRequest)
        {
            return _server.CrossServerService.WarpPlayerByName(wrapPlayerByNameRequest);
        }

        public SummonPlayerByNameResponse SummonPlayerByName(SummonPlayerByNameRequest summonPlayerByNameRequest)
        {
            return _server.CrossServerService.SummonPlayerByName(summonPlayerByNameRequest);
        }

        public DisconnectPlayerByNameResponse DisconnectPlayerByName(DisconnectPlayerByNameRequest request)
        {
            return _server.CrossServerService.DisconnectPlayerByName(request);
        }

        public void DisconnectAll(DisconnectAllRequest disconnectAllRequest)
        {
            _server.DisconnectAll(disconnectAllRequest);
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

        public void BatchSyncPlayer(List<SyncProto.PlayerSaveDto> data)
        {
            _server.CharacterManager.BatchUpdate(data);
        }

        public void SyncPlayer(PlayerSaveDto data)
        {
            _server.CharacterManager.Update(data);
        }

        public AddBuddyResponse SendAddBuddyRequest(AddBuddyRequest request)
        {
            return _server.BuddyManager.AddBuddyByName(request);
        }

        public AddBuddyResponse SendAddBuddyRequest(AddBuddyByIdRequest request)
        {
            return _server.BuddyManager.AddBuddyById(request);
        }

        public void SendBuddyChat(BuddyChatRequest request)
        {
            _server.BuddyManager.BuddyChat(request);
        }

        public void SendBuddyMessage(SendBuddyNoticeMessageDto request)
        {
            _server.BuddyManager.BroadcastNoticeMessage(request);
        }

        public DeleteBuddyResponse SendDeleteBuddy(DeleteBuddyRequest request)
        {
            return _server.BuddyManager.DeleteBuddy(request);
        }

        public SendWhisperMessageResponse SendWhisper(SendWhisperMessageRequest request)
        {
            return _server.BuddyManager.SendWhisper(request);
        }

        public GetLocationResponse GetLocation(GetLocationRequest request)
        {
            return _server.BuddyManager.GetLocation(request);
        }


        public void ShutdownMaster(ShutdownMasterRequest request)
        {
            _ = _server.Shutdown(request.DelaySeconds);
            _server.DropWorldMessage(0, $"服务器将在 {TimeSpan.FromSeconds(request.DelaySeconds).ToString()} 后停止。");
        }

        public void CompleteChannelShutdown(string serverName)
        {
            _server.CompleteChannelShutdown(serverName);
        }

        public void SaveAll(Empty empty)
        {
            _server.Transport.BroadcastMessage(BroadcastType.SaveAll, new Empty());
        }

        public void SendYellowTip(YellowTipRequest yellowTipRequest)
        {
            _server.DropYellowTip(yellowTipRequest.Message, yellowTipRequest.OnlyGM);
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

        public void SendEarnTitleMessage(EarnTitleMessageRequest data)
        {
            _server.DropEarnTitleMessage(data.Message, data.OnlyGM);
        }
    }
}
