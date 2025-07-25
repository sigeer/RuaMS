using Application.Core.Game.Players;
using Application.Core.Game.TheWorld;
using Application.Core.Game.Trades;
using Application.Core.Login;
using Application.Core.Login.ServerData;
using Application.Core.Login.Services;
using Application.Core.ServerTransports;
using Application.Shared.Login;
using Application.Shared.MapObjects;
using Application.Shared.Message;
using Application.Shared.Models;
using Application.Shared.Net;
using Application.Shared.Team;
using AutoMapper;
using BaseProto;
using CashProto;
using Dto;
using ItemProto;
using net.server;
using server.expeditions;
using System.Net;
using System.Text;
using tools;
using tools.packets;

namespace Application.Core.Channel.InProgress
{
    /// <summary>
    /// 登录服务器 与 频道服务器在同一个进程中时，直接与MasterServer交互
    /// </summary>
    public class LocalChannelServerTransport : IChannelServerTransport
    {
        readonly LoginService _loginService;
        readonly MasterServer _server;
        readonly StorageService _storageService;
        readonly ItemService _itemService;
        readonly NoteManager _noteService;
        readonly ShopService _shopManager;
        readonly MessageService _msgService;
        readonly RankService _rankService;
        readonly InvitationService _invitationService;
        readonly IExpeditionService _expeditionService;
        readonly ResourceDataManager _resourceService;
        readonly IMapper _mapper;
        /// <summary>
        /// 后期移除，逐步合并到MasterServer中去
        /// </summary>
        World _world => Server.getInstance().getWorld(0);

        public LocalChannelServerTransport(
            MasterServer server,
            LoginService loginService,
            StorageService storageService,
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
            _storageService = storageService;
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

        public Task<Config.RegisterServerResult> RegisterServer(WorldChannelServer server, List<WorldChannel> channels)
        {
            if (!_server.IsRunning)
                return Task.FromResult(new Config.RegisterServerResult() { StartChannel = -1, Message = "中心服务器未启动" });

            foreach (var item in channels)
            {
                _world.addChannel(item);
            }

            var channelId = _server.AddChannel(new InternalWorldChannel(server, channels));
            return Task.FromResult(new Config.RegisterServerResult
            {
                StartChannel = channelId,
                Coupon = _server.CouponManager.GetConfig(),
                Config = _server.GetWorldConfig()
            });
        }

        public void DropWorldMessage(int type, string message)
        {
            _world.dropMessage(type, message);
        }

        public long GetCurrentTime()
        {
            return _server.getCurrentTime();
        }

        public int GetCurrentTimestamp()
        {
            return _server.getCurrentTimestamp();
        }

        public DateTimeOffset GetServerupTime()
        {
            return Server.uptime;
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

        public Task<bool> RemoveServer(WorldChannel server)
        {
            return Task.FromResult(true);
        }

        public void BroadcastMessage(Packet p)
        {
            _server.BroadcastWorldMessage(p);
        }

        public void BroadcastGMMessage(Packet p)
        {
            _server.BroadcastWorldGMPacket(p);
        }

        public void SendTimer(int seconds)
        {
            foreach (var victim in Server.getInstance().getWorld(0).getPlayerStorage().GetAllOnlinedPlayers())
            {
                victim.sendPacket(PacketCreator.getClock(seconds));
            }
        }

        public void RemoveTimer()
        {
            foreach (var victim in Server.getInstance().getWorld(0).getPlayerStorage().GetAllOnlinedPlayers())
            {
                victim.sendPacket(PacketCreator.removeClock());
            }
        }


        public int? FindPlayerShopChannel(int ownerId)
        {
            IPlayerShop? ps = null;
            foreach (var ch in Server.getInstance().getWorld(0).getChannels())
            {
                ps = ch.PlayerShopManager.GetPlayerShop(Shared.Items.PlayerShopType.HiredMerchant, ownerId);
                if (ps != null)
                    break;
            }

            if (ps == null)
                return null;

            return ps.Channel;
        }

        public void SendAccountLogout(int accountId)
        {
            _server.UpdateAccountState(accountId, LoginStage.LOGIN_NOTLOGGEDIN);
        }

        public IPEndPoint GetChannelEndPoint(int channel)
        {
            return _world.getChannel(channel).getIP();
        }

        public void NotifyPartner(int id)
        {
            IPlayer? player = null;
            int partnerId = 0;
            IPlayer? partner = null;
            foreach (var ch in _world.getChannels())
            {
                player = ch.Players.getCharacterById(id);
                if (player != null)
                {
                    partnerId = player.PartnerId;
                }
            }

            foreach (var ch in _world.getChannels())
            {
                partner = ch.Players.getCharacterById(partnerId);
                if (partner != null)
                    break;
            }

            if (player != null && partner != null)
            {
                player.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(partner.Id, partner.getMapId()));
                partner.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(player.getId(), player.getMapId()));
            }

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

        public bool WarpPlayer(string name, int? channel, int mapId, int? portal)
        {
            return _server.WarpPlayer(name, channel, mapId, portal);

        }

        public string LoadExpeditionInfo()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var ch in _world.Channels)
            {
                sb.Append("==== 频道");
                sb.Append(ch.getId());
                sb.Append(" ====");
                sb.Append("\r\n\r\n");
                List<Expedition> expeds = ch.getExpeditions();
                if (expeds.Count == 0)
                {
                    sb.Append("无");
                    continue;
                }

                int id = 0;
                foreach (Expedition exped in expeds)
                {
                    id++;
                    sb.Append("> Expedition " + id);
                    sb.Append(">> Type: " + exped.getType().ToString());
                    sb.Append(">> Status: " + (exped.isRegistering() ? "REGISTERING" : "UNDERWAY"));
                    sb.Append(">> Size: " + exped.getMembers().Count);
                    sb.Append(">> Leader: " + exped.getLeader().getName());
                    int memId = 2;
                    foreach (var e in exped.getMembers())
                    {
                        if (exped.isLeader(e.Key))
                        {
                            continue;
                        }
                        sb.Append(">>> Member " + memId + ": " + e.Value);
                        memId++;
                    }
                }
            }
            return sb.ToString();
        }

        public Dto.PlayerGetterDto? GetPlayerData(string clientSession, int channelId, int cid)
        {
            return _loginService.PlayerLogin(clientSession, channelId, cid);
        }

        public int GetAccountCharacterCount(int accId)
        {
            return _server.GetAccountCharacterCount(accId);
        }

        public bool CheckCharacterName(string name)
        {
            return _server.CheckCharacterName(name);
        }

        public void UpdateAccountChracterByAdd(int accountId, int id)
        {
            _server.UpdateAccountChracterByAdd(accountId, id);
        }

        public void SendPlayerObject(Dto.PlayerSaveDto characterValueObject)
        {
            _server.CharacterManager.Update(characterValueObject);
        }

        public void SendRemovePlayerIncomingInvites(int id)
        {
            _invitationService.RemovePlayerInvitation(id);
        }

        public void SendBuffObject(int v, Dto.PlayerBuffSaveDto playerBuffSaveDto)
        {
            _server.BuffManager.SaveBuff(v, playerBuffSaveDto);
        }
        public Dto.PlayerBuffSaveDto GetBuffObject(int id)
        {
            return _server.BuffManager.Get(id);
        }

        public void SetPlayerOnlined(int id, int v)
        {
            _loginService.SetPlayerLogedIn(id, v);
        }

        public void CallSaveDB()
        {
            _ = _storageService.CommitAllImmediately();
        }

        public Dto.DropAllDto RequestAllReactorDrops()
        {
            return _itemService.LoadAllReactorDrops();
        }

        public int[] RequestReactorSkillBooks()
        {
            return _itemService.LoadReactorSkillBooks();
        }

        public Dto.SpecialCashItemListDto RequestSpecialCashItems()
        {
            return _itemService.LoadSpecialCashItems();
        }


        public GetMyGiftsResponse LoadPlayerGifts(GetMyGiftsRequest request)
        {
            return _server.GiftManager.LoadGifts(request);
        }
        public void ClearGifts(int[] giftIdArray)
        {
            _itemService.ClearGifts(giftIdArray);
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

        public int[] GetCardTierSize()
        {
            return _itemService.GetCardTierSize();
        }

        public void SendUnbanAccount(string playerName)
        {
            _loginService.UnBanAccount(playerName);
        }

        public void AddReport(int fromId, int toId, int reason, string description, string chatLog)
        {
            _msgService.AddReport(fromId, toId, reason, description, chatLog);
        }

        public Rank.RankCharacterList LoadPlayerRanking(int topCount)
        {
            return _rankService.LoadPlayerRanking(topCount);
        }

        public void SendToggleCoupon(int v)
        {
            _server.CouponManager.ToggleCoupon(v);
        }
        public void UpdateAccount(AccountCtrl accountEntity)
        {
            _server.AccountManager.UpdateAccount(accountEntity);
        }

        public Dto.CreateCharResponseDto SendNewPlayer(Dto.NewPlayerSaveDto data)
        {
            return new Dto.CreateCharResponseDto { Code = _server.CharacterManager.CreatePlayerDB(data) };
        }

        public Dto.CreateCharCheckResponse CreatePlayerCheck(Dto.CreateCharCheckRequest request)
        {
            return new Dto.CreateCharCheckResponse()
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
        public Dto.TeamDto CreateTeam(int playerId)
        {
            return _server.TeamManager.CreateTeam(playerId);
        }

        public Dto.UpdateTeamResponse SendUpdateTeam(int teamId, PartyOperation operation, int fromId, int toId)
        {
            return _server.TeamManager.UpdateParty(teamId, operation, fromId, toId);
        }

        public void SendTeamChat(string name, string chattext)
        {
            _server.TeamManager.SendTeamChat(name, chattext);
        }

        public Dto.GetTeamResponse GetTeam(int party)
        {
            return new Dto.GetTeamResponse() { Model = _server.TeamManager.GetTeamFull(party) };
        }




        #endregion

        #region Guild & Alliance
        public Dto.GetGuildResponse GetGuild(int id)
        {
            return new Dto.GetGuildResponse() { Model = _server.GuildManager.GetGuildFull(id) };
        }

        public Dto.GetGuildResponse CreateGuild(string guildName, int playerId, int[] members)
        {
            return new Dto.GetGuildResponse { Model = _server.GuildManager.CreateGuild(guildName, playerId, members) };
        }

        public Dto.CreateAllianceCheckResponse CreateAllianceCheck(Dto.CreateAllianceCheckRequest request)
        {
            return _server.GuildManager.CreateAllianceCheck(request);
        }
        public Dto.GetAllianceResponse CreateAlliance(int[] masters, string allianceName)
        {
            return new Dto.GetAllianceResponse { Model = _server.GuildManager.CreateAlliance(masters, allianceName) };
        }


        public Dto.GetAllianceResponse GetAlliance(int id)
        {
            return new Dto.GetAllianceResponse { Model = _server.GuildManager.GetAllianceFull(id) };
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

        public void SendUpdateGuildGP(Dto.UpdateGuildGPRequest request)
        {
            _server.GuildManager.UpdateGuildGP(request);
        }

        public void SendUpdateGuildRankTitle(Dto.UpdateGuildRankTitleRequest request)
        {
            _server.GuildManager.UpdateGuildRankTitle(request);
        }

        public void SendUpdateGuildNotice(Dto.UpdateGuildNoticeRequest request)
        {
            _server.GuildManager.UpdateGuildNotice(request);
        }

        public void SendUpdateGuildCapacity(Dto.UpdateGuildCapacityRequest request)
        {
            _server.GuildManager.IncreseGuildCapacity(request);
        }

        public void SendUpdateGuildEmblem(Dto.UpdateGuildEmblemRequest request)
        {
            _server.GuildManager.UpdateGuildEmblem(request);
        }

        public void SendGuildDisband(Dto.GuildDisbandRequest request)
        {
            _server.GuildManager.DisbandGuild(request);
        }

        public void SendChangePlayerGuildRank(Dto.UpdateGuildMemberRankRequest request)
        {
            _server.GuildManager.ChangePlayerGuildRank(request);
        }

        public void SendGuildExpelMember(Dto.ExpelFromGuildRequest request)
        {
            _server.GuildManager.GuildExpelMember(request);
        }

        public void SendPlayerLeaveGuild(Dto.LeaveGuildRequest request)
        {
            _server.GuildManager.PlayerLeaveGuild(request);
        }

        public void SendPlayerJoinGuild(Dto.JoinGuildRequest request)
        {
            _server.GuildManager.PlayerJoinGuild(request);
        }

        public void SendGuildJoinAlliance(Dto.GuildJoinAllianceRequest request)
        {
            _server.GuildManager.GuildJoinAlliance(request);
        }

        public void SendGuildLeaveAlliance(Dto.GuildLeaveAllianceRequest request)
        {
            _server.GuildManager.GuildLeaveAlliance(request);
        }

        public void SendAllianceExpelGuild(Dto.AllianceExpelGuildRequest request)
        {
            _server.GuildManager.AllianceExpelGuild(request);
        }

        public void SendChangeAllianceLeader(Dto.AllianceChangeLeaderRequest request)
        {
            _server.GuildManager.ChangeAllianceLeader(request);
        }

        public void SendChangePlayerAllianceRank(Dto.ChangePlayerAllianceRankRequest request)
        {
            _server.GuildManager.ChangePlayerAllianceRank(request);
        }

        public void SendIncreaseAllianceCapacity(Dto.IncreaseAllianceCapacityRequest request)
        {
            _server.GuildManager.IncreaseAllianceCapacity(request);
        }

        public void SendUpdateAllianceRankTitle(Dto.UpdateAllianceRankTitleRequest request)
        {
            _server.GuildManager.UpdateAllianceRankTitle(request);
        }

        public void SendUpdateAllianceNotice(Dto.UpdateAllianceNoticeRequest request)
        {
            _server.GuildManager.UpdateAllianceNotice(request);
        }

        public void SendAllianceDisband(Dto.DisbandAllianceRequest request)
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

        public void SendInvitation(Dto.CreateInviteRequest request)
        {
            _invitationService.AddInvitation(request);
        }

        public void AnswerInvitation(Dto.AnswerInviteRequest request)
        {
            _invitationService.AnswerInvitation(request);
        }

        public void RegisterExpedition(Dto.ExpeditionRegistry request)
        {
            _expeditionService.RegisterExpedition(request);
        }

        public Dto.ExpeditionCheckResponse CanStartExpedition(Dto.ExpeditionCheckRequest expeditionCheckRequest)
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

        public void SendSetFly(SetFlyRequest setFlyRequest)
        {
            _server.AccountManager.SetFly(setFlyRequest);
        }

        public void SendReloadEvents(ReloadEventsRequest reloadEventsRequest)
        {
            _server.Transport.BroadcastMessage(BroadcastType.OnEventsReloaded, new ReloadEventsResponse { Code = 0, Request = reloadEventsRequest });
        }

        public void BroadcastMessage(SendTextMessage data)
        {
            _server.Transport.BroadcastMessage(BroadcastType.OnMessage, data);
        }

        public void BroadcastTV(ItemProto.CreateTVMessageRequest request)
        {
            _itemService.BroadcastTV(request);
        }

        public void SendItemMegaphone(ItemProto.UseItemMegaphoneRequest request)
        {
            _itemService.BroadcastItemMegaphone(request);
        }

        public void FinishTransaction(ItemProto.FinishTransactionRequest finishTransactionRequest)
        {
            _server.ItemTransactionManager.Finish(finishTransactionRequest);
        }

        public DropAllDto RequestDropData()
        {
            return _itemService.LoadMobDropDto();
        }

        public QueryDropperByItemResponse RequestWhoDrops(QueryDropperByItemRequest request)
        {
            return _itemService.LoadWhoDrops(request);
        }

        public QueryMonsterCardDataResponse RequestMonsterCardData()
        {
            return _itemService.LoadMonsterCard();
        }

        public QueryRankedGuildsResponse RequestRankedGuilds()
        {
            return _server.GuildManager.LoadRankedGuilds();
        }

        public GetPLifeByMapIdResponse RequestPLifeByMapId(GetPLifeByMapIdRequest request)
        {
            return _resourceService.LoadMapPLife(request);
        }

        public void SendCreatePLife(CreatePLifeRequest createPLifeRequest)
        {
            _resourceService.CreatePLife(createPLifeRequest);
        }

        public void SendRemovePLife(RemovePLifeRequest removePLifeRequest)
        {
            _resourceService.RemovePLife(removePLifeRequest);
        }

        public void SendBuyCashItem(BuyCashItemRequest buyCashItemRequest)
        {
            _server.CashShopDataManager.BuyCashItem(buyCashItemRequest);
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

        public void CompleteTakeItem(TakeItemSubmit request)
        {
            _server.ItemTransactionManager.CompleteTakeItem(request);
        }

        public StoreItemsResponse SaveItems(StoreItemsRequest request)
        {
            return _server.ItemFactoryManager.Store(request);
        }

        public LoadItemsFromStoreResponse LoadItemFromStore(LoadItemsFromStoreRequest request)
        {
            return _server.ItemFactoryManager.LoadItems(request);
        }
    }
}
