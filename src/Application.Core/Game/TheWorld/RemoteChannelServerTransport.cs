using Application.Core.model;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using net.server;
using RemoteService;

namespace Application.Core.Game.TheWorld
{
    public class RemoteChannelServerTransport : IChannelSeverTransport
    {
        public ServerCenterService.ServerCenterServiceClient _client { get; private set; }
        GrpcChannel _centerChannel;
        readonly ILogger log;

        public RemoteChannelServerTransport(string host)
        {
            _centerChannel = GrpcChannel.ForAddress(host);
            _client = new RemoteService.ServerCenterService.ServerCenterServiceClient(_centerChannel);
        }

        public async Task<int> RegisterChannel(IWorldChannel worldChannel)
        {
            var channelIdWrapper = await _client!.AddChannelAsync(new ChannelInfo { 
                Id = worldChannel.InstanceId, 
                Host = worldChannel.ServerConfig.Host, 
                Port = worldChannel.ServerConfig.Port, 
                GrpcAddress = worldChannel.ServerConfig.GrpcServiceEndPoint
            });
            if (channelIdWrapper != null && channelIdWrapper.ChannelId > 0)
            {
                return channelIdWrapper.ChannelId;
            }
            else
            {
                return -1;
            }
        }
        public async Task UnRegisterChannel(string id)
        {
            if (_centerChannel == null)
                return;

            var channelIdWrapper = await _client!.RemoveChannelAsync(new ChannelInfo { Id = id });
            log.Information("频道服务器卸载成功");
            await _centerChannel.ShutdownAsync();
            _centerChannel.Dispose();
            _centerChannel = null;
        }

        public long GetServerCurrentTime()
        {
            return _client.GetCurrentTime(new Empty()).Time.ToDateTimeOffset().ToUnixTimeMilliseconds();
        }

        public void BroadcastMessage(Packet p)
        {
            _client.BroadcastMessage(new MessagePacket
            {
                Data = ByteString.CopyFrom(p.getBytes()),
                PlayerType = PlayerType.Default
            });
        }

        public void BroadcastGMMessage(Packet p)
        {
            _client.BroadcastMessage(new MessagePacket
            {
                Data = ByteString.CopyFrom(p.getBytes()),
                PlayerType = PlayerType.Gm
            });
        }

        public int GetServerCurrentTimestamp()
        {
            return (int)_client.GetServerTimestamp(new Empty()).Duration.ToTimeSpan().TotalSeconds;
        }

        public DateTimeOffset GetServerUpTime()
        {
            return Server.uptime;
        }

        public CoupleIdPair? GetRelationshipCouple(int cathedralId)
        {
            throw new NotImplementedException();
        }

        public void PutMarriageQueued(int weddingId, bool isCathedral, bool isPremium, int groomId, int bridgeId)
        {
            throw new NotImplementedException();
        }

        public bool IsMarriageQueued(int weddingId)
        {
            throw new NotImplementedException();
        }

        public CoupleIdPair? GetMarriageQueuedCouple(int weddingId)
        {
            throw new NotImplementedException();
        }

        public KeyValuePair<bool, HashSet<int>> RemoveMarriageQueued(int marriageId)
        {
            throw new NotImplementedException();
        }

        public void DropMessage(int type, string message)
        {
            throw new NotImplementedException();
        }

        public float GetWorldMobRate()
        {
            return 1;
        }

        public void RemoveGuildQueued(int guildId)
        {
            throw new NotImplementedException();
        }

        public bool IsGuildQueued(int guildId)
        {
            throw new NotImplementedException();
        }

        public void PutGuildQueued(int guildId)
        {
            throw new NotImplementedException();
        }

        public int GetTransportationTime(double travelTime)
        {
            throw new NotImplementedException();
        }

        public void DisconnectPlayers(IEnumerable<int> playerIdList)
        {
            throw new NotImplementedException();
            //foreach (int cid in playersAway)
            //{
            //    var chr = wserv.Players.getCharacterById(cid);
            //    if (chr != null && chr.IsOnlined)
            //    {
            //        chr.getClient().forceDisconnect();
            //    }
            //}
        }

        public void ResetDisabledServerMessages()
        {
            throw new NotImplementedException();
        }

        public int GetPlayerNpcMapStep(int mapid)
        {
            throw new NotImplementedException();
        }

        public void SetPlayerNpcMapStep(int mapid, int step)
        {
            throw new NotImplementedException();
        }

        public int GetPlayerNpcMapPodiumData(int mapid)
        {
            throw new NotImplementedException();
        }

        public void SetPlayerNpcMapPodiumData(int mapid, int podium)
        {
            throw new NotImplementedException();
        }
        public void BroadcastMessage(IEnumerable<int> playerIdList, Packet p)
        {
            throw new NotImplementedException();
        }

        public void ResetFamilyDailyReps()
        {
            throw new NotImplementedException();
        }
    }
}
