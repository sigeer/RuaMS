using Application.Core.Managers;
using Application.Shared.Servers;
using Grpc.Net.Client;
using net.server;
using server.life;
using tools;
using World2Channel;

namespace Application.Core.Game.TheWorld
{
    public interface IWorldServerTransport : IServerTransport
    {
        IPlayer? GetChannelPlayer(int channel, int playerId);
        IPlayer? GetChannelPlayerByName(int channel, string playerName);
        IPlayer? FindPlayer(int playerId);
        IPlayer? FindPlayerByName(string playerName);
        bool AddMapSpawnPoint(int mapId, int mobId, Point pos, int cy, int rx0, int rx1, int fh, int mobTime);
        void RemoveMapSpawnPoint(int mapId, int mobId, int x, int y);
        void AddPnpc(int mapId, int npcId, Point pos, int cy, int rx0, int rx1, int fh);
        void RemovePnpc(int mapId, int npcId);
        Task Shutdown();
        void SetServerMessage(string message);

        void SendDueyNotification(string playerName);
    }

    public class WorldServerTransport : IWorldServerTransport
    {
        Dictionary<string, GrpcChannel> _channelDic = new Dictionary<string, GrpcChannel>();
        Dictionary<string, World2ChannelService.World2ChannelServiceClient> _clientDic = new Dictionary<string, World2ChannelService.World2ChannelServiceClient>();

        private World2ChannelService.World2ChannelServiceClient GetClient(ActualServerConfig config)
        {
            if (_clientDic.TryGetValue(config.GrpcServiceEndPoint, out var client))
                return client;

            _channelDic[config.GrpcServiceEndPoint] = GrpcChannel.ForAddress(config.GrpcServiceEndPoint);
            _clientDic[config.GrpcServiceEndPoint] = new World2ChannelService.World2ChannelServiceClient(_channelDic[config.GrpcServiceEndPoint]);
            return _clientDic[config.GrpcServiceEndPoint];
        }
        public bool AddMapSpawnPoint(int mapId, int mobId, Point pos, int cy, int rx0, int rx1, int fh, int mobTime)
        {
            foreach (var ch in Server.getInstance().ChannelList)
            {
                if (ch is InternalWorldChannel internalWorldChannel)
                {
                    var mob = LifeFactory.getMonster(mobId);
                    if (mob == null)
                        return false;

                    mob.setPosition(pos);
                    mob.setCy(cy);
                    mob.setRx0(rx0);
                    mob.setRx1(rx1);
                    mob.setFh(fh);

                    var map = internalWorldChannel.WorldChannel.getMapFactory().getMap(mapId);
                    map.addMonsterSpawn(mob, mobTime, -1);
                    map.addAllMonsterSpawn(mob, mobTime, -1);
                }
                else
                {
                    var client = GetClient(ch.ServerConfig);
                    client.AddMonsterSpawn(new AddMonsterSpawnMessage
                    {
                        MapId = mapId,
                        MobId = mobId,
                        PosX = pos.X,
                        PoxY = pos.Y,
                        Cy = cy,
                        Rx0 = rx0,
                        Rx1 = rx1,
                        Fh = fh,
                        MobTime = mobTime
                    });
                }
            }
            return true;
        }

        public void AddPnpc(int mapId, int npcId, Point pos, int cy, int rx0, int rx1, int fh)
        {
            foreach (var ch in Server.getInstance().ChannelList)
            {
                if (ch is InternalWorldChannel internalWorldChannel)
                {
                    var npc = LifeFactory.getNPC(npcId);
                    npc.setPosition(pos);
                    npc.setCy(cy);
                    npc.setRx0(rx0);
                    npc.setRx1(rx1);
                    npc.setFh(fh);

                    var map = internalWorldChannel.WorldChannel.getMapFactory().getMap(mapId);
                    map.addMapObject(npc);
                    map.broadcastMessage(PacketCreator.spawnNPC(npc));
                }
                else
                {

                }
            }
        }

        public void BroadcastGMMessage(Packet p)
        {
            throw new NotImplementedException();
        }

        public void BroadcastMessage(Packet p)
        {
            throw new NotImplementedException();
        }

        public void BroadcastMessage(IEnumerable<int> playerIdList, Packet p)
        {
            throw new NotImplementedException();
        }

        public void DropMessage(int type, string message)
        {
            throw new NotImplementedException();
        }

        public IPlayer? FindPlayer(int playerId)
        {
            return AllPlayerStorage.GetOrAddCharacterById(playerId);
        }

        public IPlayer FindPlayerByName(string playerName)
        {
            throw new NotImplementedException();
        }

        public IPlayer? GetChannelPlayer(int channel, int playerId)
        {
            var channelObj = Server.getInstance().GetChannel(channel);
            if (channelObj is InternalWorldChannel internalWorldChannel)
            {
                return internalWorldChannel.WorldChannel.getPlayerStorage().getCharacterById(playerId);
            }

        }

        public IPlayer? GetChannelPlayerByName(int channel, string playerName)
        {
            return Server.getInstance().getChannel(0, channel).getPlayerStorage().getCharacterByName(playerName);
        }

        public long GetServerCurrentTime()
        {
            throw new NotImplementedException();
        }

        public int GetServerCurrentTimestamp()
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset GetServerUpTime()
        {
            throw new NotImplementedException();
        }

        public void RemoveMapSpawnPoint(int mapId, int mobId, int x, int y)
        {
            foreach (var ch in Server.getInstance().getWorld(0).getChannels())
            {
                var map = ch.getMapFactory().getMap(mapId);
                map.removeMonsterSpawn(mobId, x, y);
                map.removeAllMonsterSpawn(mobId, x, y);
            }
        }

        public void RemovePnpc(int mapId, int npcId)
        {
            foreach (var ch in Server.getInstance().getWorld(0).getChannels())
            {
                var map = ch.getMapFactory().getMap(mapId);

                map.destroyNPC(npcId);
            }
        }

        public void SetServerMessage(string message)
        {
            foreach (var ch in Server.getInstance().getWorld(0).getChannels())
            {
                ch.setServerMessage(message);
            }
        }

        public async Task Shutdown()
        {
            foreach (var ch in Server.getInstance().getWorld(0).getChannels())
            {
                await ch.Shutdown();
            }
        }



        public void SendDueyNotification(string playerName)
        {
            foreach (var ch in Server.getInstance().ChannelList)
            {
                if (ch is InternalWorldChannel internalWorldChannel)
                {
                    var player = internalWorldChannel.WorldChannel.Players.getCharacterByName(playerName);
                    if (player != null && player.isLoggedinWorld())
                        ItemManager.ShowDueyNotification(player);
                }
                else
                {
                    var client = GetClient(ch.ServerConfig);
                    client.SendDueyNotification(new SendDueyNotificationMessage { Name = playerName });
                }
            }
        }
    }
}
