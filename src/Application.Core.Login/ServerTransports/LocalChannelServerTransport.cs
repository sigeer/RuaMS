using Application.Core.Datas;
using Application.Core.Game.Players;
using Application.Core.Game.Relation;
using Application.Core.Game.TheWorld;
using Application.Core.Game.Trades;
using Application.Core.Login.Datas;
using Application.Core.Login.Services;
using Application.Core.model;
using Application.Core.Servers;
using Application.Core.ServerTransports;
using Application.Shared.Configs;
using Application.Shared.Dto;
using Application.Shared.Login;
using Application.Shared.MapObjects;
using Application.Shared.Net;
using net.packet;
using net.server;
using net.server.guild;
using server.expeditions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using tools;
using tools.packets;

namespace Application.Core.Login.ServerTransports
{
    /// <summary>
    /// 登录服务器 与 频道服务器在同一个进程中时，直接与MasterServer交互
    /// </summary>
    public class LocalChannelServerTransport : IChannelServerTransport
    {
        readonly LoginService _loginService;
        readonly IMasterServer _server;
        readonly CharacterManager _chrManager;
        /// <summary>
        /// 后期移除，逐步合并到MasterServer中去
        /// </summary>
        IWorld _world => Server.getInstance().getWorld(0);

        public LocalChannelServerTransport(IMasterServer server, LoginService loginService, CharacterManager chrManager)
        {
            _server = server;
            _loginService = loginService;
            _chrManager = chrManager;
        }

        public Task<int> RegisterServer(IWorldChannel server)
        {
            var channelId = _world.addChannel(server);
            server.UpdateWorldConfig(new WorldConfigPatch
            {
                MobRate = _server.MobRate,
                MesoRate = _server.MesoRate,
                ExpRate = _server.ExpRate,
                DropRate = _server.DropRate,
                BossDropRate = _server.BossDropRate,
                QuestRate = _server.QuestRate,
                TravelRate = _server.TravelRate,
                FishingRate = _server.FishingRate,
                ServerMessage = _server.ServerMessage
            });
            _server.AddChannel(new InternalWorldChannel(server));
            return Task.FromResult(channelId);
        }

        public void DisconnectPlayers(IEnumerable<int> playerIdList)
        {
            foreach (var playerId in playerIdList)
            {
                var chr = _world.getPlayerStorage().getCharacterById(playerId);
                if (chr != null && chr.IsOnlined)
                {
                    chr.getClient().ForceDisconnect();
                }
            }
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

        public CoupleIdPair? GetMarriageQueuedCouple(int weddingId)
        {
            return _server.WeddingInstance.GetMarriageQueuedCouple(weddingId);
        }

        public CoupleIdPair? GetRelationshipCouple(int cathedralId)
        {
            return _server.WeddingInstance.GetRelationshipCouple(cathedralId);
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


        public bool IsMarriageQueued(int weddingId)
        {
            return _server.WeddingInstance.IsMarriageQueued(weddingId);
        }



        public void PutMarriageQueued(int weddingId, bool isCathedral, bool isPremium, int groomId, int bridgeId)
        {
            _server.WeddingInstance.PutMarriageQueued(weddingId, isCathedral, isPremium, groomId, bridgeId);
        }


        public KeyValuePair<bool, HashSet<int>> RemoveMarriageQueued(int marriageId)
        {
            return _server.WeddingInstance.RemoveMarriageQueued(marriageId);
        }

        public int CreateRelationship(int groomId, int brideId)
        {
            return _server.WeddingInstance.CreateRelationship(groomId, brideId);
        }

        public int GetRelationshipId(int playerId)
        {
            return _server.WeddingInstance.GetRelationshipId(playerId);
        }

        public void DeleteRelationship(int playerId, int partnerId)
        {
            _server.WeddingInstance.DeleteRelationship(playerId, partnerId);
        }

        public KeyValuePair<bool, bool>? GetMarriageQueuedLocation(int marriageId)
        {
            return _server.WeddingInstance.GetMarriageQueuedLocation(marriageId);
        }

        public bool AddMarriageGuest(int marriageId, int playerId)
        {
            return _server.WeddingInstance.AddMarriageGuest(marriageId, playerId);
        }

        public CoupleIdPair? GetWeddingCoupleForGuest(int guestId, bool cathedral)
        {
            return _server.WeddingInstance.GetWeddingCoupleForGuest(guestId, cathedral);
        }

        public void SendWorldConfig(WorldConfigPatch updatePatch)
        {
            _server.UpdateWorldConfig(updatePatch);
        }

        public Task<bool> RemoveServer(IWorldChannel server)
        {
            return Task.FromResult(true);
        }

        public ITeam CreateTeam(int playerId)
        {
            throw new NotImplementedException();
        }

        public void SetPlayerNpcMapPodiumData(int mapId, int podumData)
        {
            _world.setPlayerNpcMapPodiumData(mapId, podumData);
        }

        public int GetPlayerNpcMapPodiumData(int mapId)
        {
            return _world.getPlayerNpcMapPodiumData(mapId);
        }

        public void SetPlayerNpcMapStep(int mapId, int step)
        {
            _world.setPlayerNpcMapStep(mapId, step);
        }

        public int GetPlayerNpcMapStep(int mapId)
        {
            return _world.getPlayerNpcMapStep(mapId);
        }

        public void RequestRemovePlayerNpc(int mapId, IEnumerable<int> playerNpcObjectId)
        {
            foreach (var ch in Server.getInstance().getChannelsFromWorld(0))
            {
                var map = ch.getMapFactory().getMap(mapId);


                foreach (var pn in playerNpcObjectId)
                {
                    map.removeMapObject(pn);
                    map.broadcastMessage(PacketCreator.removeNPCController(pn));
                    map.broadcastMessage(PacketCreator.removePlayerNPC(pn));
                }
            }
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

        public List<OwlSearchResult> OwlSearch(int itemId)
        {
            List<OwlSearchResult> hmsAvailable = new();

            foreach (var ch in Server.getInstance().getWorld(0).getChannels())
            {
                foreach (var hm in ch.HiredMerchantController.getActiveMerchants())
                {
                    List<PlayerShopItem> itemBundles = hm.sendAvailableBundles(itemId);

                    foreach (PlayerShopItem mpsi in itemBundles)
                    {
                        hmsAvailable.Add(new OwlSearchResult
                        {
                            Bundles = mpsi.getBundles(),
                            Price = mpsi.getPrice(),
                            Channel = hm.Channel,
                            Description = hm.getDescription(),
                            ItemQuantity = mpsi.getItem().getQuantity(),
                            MapId = hm.getMapId(),
                            OwnerId = hm.getOwnerId(),
                            OwnerName = hm.getOwner()
                        });
                    }
                }
            }

            foreach (PlayerShop ps in Server.getInstance().getWorld(0).getActivePlayerShops())
            {
                List<PlayerShopItem> itemBundles = ps.sendAvailableBundles(itemId);

                foreach (PlayerShopItem mpsi in itemBundles)
                {
                    hmsAvailable.Add(new OwlSearchResult
                    {
                        Bundles = mpsi.getBundles(),
                        Price = mpsi.getPrice(),
                        Channel = ps.Channel,
                        Description = ps.getDescription(),
                        ItemQuantity = mpsi.getItem().getQuantity(),
                        MapId = ps.getMapId(),
                        OwnerId = ps.getOwner().Id,
                        OwnerName = ps.getOwner().Name
                    });
                }
            }
            hmsAvailable = hmsAvailable.OrderBy(x => x.Price).Take(200).ToList();
            return hmsAvailable;
        }

        public PlayerShopDto? SendOwlWarp(int mapId, int ownerId, int searchItem)
        {
            IPlayerShop? ps = null;
            foreach (var ch in Server.getInstance().getWorld(0).getChannels())
            {
                ps = ch.HiredMerchantController.getHiredMerchant(ownerId);
                if (ps != null)
                    break;
            }

            if (ps == null)
                ps = Server.getInstance().getWorld(0).getPlayerShop(ownerId);

            if (ps == null || ps.getMap().getId() != mapId || !ps.hasItem(searchItem))
                return null;

            return new PlayerShopDto
            {
                MapName = ps.getMap().getMapName(),
                Channel = ps.Channel,
                IsOpen = ps.isOpen(),
                TypeName = ps.TypeName
            };
        }

        public int? FindPlayerShopChannel(int ownerId)
        {
            IPlayerShop? ps = null;
            foreach (var ch in Server.getInstance().getWorld(0).getChannels())
            {
                ps = ch.HiredMerchantController.getHiredMerchant(ownerId);
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

        public void UpdateAccountState(int accId, sbyte state)
        {
            _server.UpdateAccountState(accId, state);
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

        public void ChangePlayerAllianceRank(int targetCharacterId, bool isRaise)
        {
            foreach (var ch in _world.Channels)
            {
                var chr = ch.Players.getCharacterById(targetCharacterId);
                if (chr != null)
                {
                    var alliance = chr.getAlliance();
                    if (alliance == null)
                        return;

                    int newRank = chr.getAllianceRank() + (isRaise ? -1 : 1);
                    if (newRank < 3 || newRank > 5)
                    {
                        return;
                    }

                    chr.setAllianceRank(newRank);
                    chr.saveGuildStatus();


                    alliance.broadcastMessage(GuildPackets.getGuildAlliances(alliance), -1, -1);
                    alliance.dropMessage("'" + chr.getName() + "' has been reassigned to '" + alliance.getRankTitle(newRank) + "' in this Alliance.");
                }
            }
        }

        public CharacterValueObject? GetPlayerData(string clientSession, int cid)
        {
            return _loginService.PlayerLogin(clientSession, cid);
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

        public void SendPlayerObject(PlayerSaveDto characterValueObject)
        {
            _chrManager.Update(characterValueObject);
        }

        public void SendRemovePlayerIncomingInvites(int id)
        {
            _server.InvitationController.RemovePlayerIncomingInvites(id);
        }

        public void SendBuffObject(int v, PlayerBuffSaveDto playerBuffSaveDto)
        {
            _server.SaveBuff(v, playerBuffSaveDto);
        }

        public PlayerBuffSaveDto GetBuffObject(int id)
        {
            return _server.GetBuff(id);
        }
    }
}
