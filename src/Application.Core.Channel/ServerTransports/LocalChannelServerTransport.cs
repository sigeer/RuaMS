using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.Game.Relation;
using Application.Core.Game.TheWorld;
using Application.Core.Game.Trades;
using Application.Core.model;
using Application.Core.Servers;
using Application.Core.ServerTransports;
using Application.Shared.Configs;
using Application.Shared.MapObjects;
using Application.Shared.Relations;
using constants.id;
using net.netty;
using net.packet;
using net.server;
using Org.BouncyCastle.Asn1.X509;
using Serilog;
using server.maps;
using System.Numerics;
using System.Security.Cryptography;
using tools;

namespace Application.Core.Channel.ServerTransports
{
    /// <summary>
    /// 登录服务器 与 频道服务器在同一个进程中时，直接与MasterServer交互
    /// </summary>
    public class LocalChannelServerTransport : IChannelServerTransport
    {
        readonly IMasterServer _server;
        /// <summary>
        /// 后期移除，逐步合并到MasterServer中去
        /// </summary>
        readonly IWorld _world;
        /// <summary>
        /// 因为在同一个进程，所以能够取到masterserver和world
        /// </summary>
        /// <param name="server"></param>
        /// <param name="world"></param>
        readonly IWorldChannel _worldChannel;
        public LocalChannelServerTransport(IMasterServer server, IWorld world, IWorldChannel worldChanne)
        {
            _server = server;
            _world = world;
            _worldChannel = worldChanne;
        }

        public Task<int> RegisterServer()
        {
            var channelId = _world.addChannel(_worldChannel);
            _worldChannel.UpdateWorldConfig(new WorldConfigPatch
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
            return Task.FromResult(channelId);
            return Task.FromResult(_server.AddChannel(new InternalWorldChannel(_worldChannel)));
        }

        public void DisconnectPlayers(IEnumerable<int> playerIdList)
        {
            foreach (var playerId in playerIdList)
            {
                var chr = _world.getPlayerStorage().getCharacterById(playerId);
                if (chr != null && chr.IsOnlined)
                {
                    chr.getClient().forceDisconnect();
                }
            }
        }

        public void DropWorldMessage(int type, string message)
        {
            _world.dropMessage(type, message);
        }

        public long GetCurrentTime()
        {
            return Server.getInstance().getCurrentTime();
        }

        public int GetCurrentTimestamp()
        {
            return Server.getInstance().getCurrentTimestamp();
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
            Server.getInstance().broadcastMessage(0, p);
        }

        public void BroadcastGMMessage(Packet p)
        {
            Server.getInstance().broadcastGMMessage(0, p);
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

        public int CreateTeam(int playerId)
        {
            return _server.CreateTeam(playerId);
        }


        public void SendExpelFromParty(int partyId, int expelCid)
        {
            _server.ExpelFromParty(partyId, expelCid);
        }

        public void SendUpdateTeamGlobalData(int partyId, PartyOperation operation, int targetId, string targetName)
        {
            _server.UpdateTeamGlobalData(partyId, operation, targetId, targetName);
        }

    }
}
