using Application.Core.Game.TheWorld;
using Application.Core.model;
using Application.Core.Servers;
using Application.Core.ServerTransports;
using Application.Shared.Configs;
using net.server;
using System.Security.Cryptography;

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
        public LocalChannelServerTransport(IMasterServer server, IWorld world)
        {
            _server = server;
            _world = world;
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
            return Task.FromResult(channelId);
            return Task.FromResult(_server.AddChannel(new InternalWorldChannel(server)));
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

    }
}
