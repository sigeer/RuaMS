using Application.Core.Game.TheWorld;
using Application.Core.model;
using Application.Core.ServerTransports;
using Application.Shared.Configs;

namespace Application.Host.Channel
{
    /// <summary>
    /// 与MasterServer不在同一进程，通过grpc请求masterserver服务器
    /// </summary>
    public class RemoteChannelServerTransport : IChannelServerTransport
    {
        public bool AddMarriageGuest(int marriageId, int playerId)
        {
            throw new NotImplementedException();
        }

        public int CreateRelationship(int groomId, int brideId)
        {
            throw new NotImplementedException();
        }

        public void DeleteRelationship(int playerId, int partnerId)
        {
            throw new NotImplementedException();
        }

        public void DisconnectPlayers(IEnumerable<int> playerIdList)
        {
            throw new NotImplementedException();
        }

        public void DropWorldMessage(int type, string message)
        {
            throw new NotImplementedException();
        }

        public long GetCurrentTime()
        {
            throw new NotImplementedException();
        }

        public int GetCurrentTimestamp()
        {
            throw new NotImplementedException();
        }

        public CoupleIdPair? GetMarriageQueuedCouple(int weddingId)
        {
            throw new NotImplementedException();
        }

        public KeyValuePair<bool, bool>? GetMarriageQueuedLocation(int marriageId)
        {
            throw new NotImplementedException();
        }

        public CoupleIdPair? GetRelationshipCouple(int cathedralId)
        {
            throw new NotImplementedException();
        }

        public int GetRelationshipId(int playerId)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset GetServerupTime()
        {
            throw new NotImplementedException();
        }

        public int GetTransportationTime(double travelTime)
        {
            throw new NotImplementedException();
        }

        public CoupleIdPair? GetWeddingCoupleForGuest(int guestId, bool cathedral)
        {
            throw new NotImplementedException();
        }

        public float GetWorldMobRate()
        {
            throw new NotImplementedException();
        }

        public void UpdateWorldMobRate(float newMobRate)
        {
            throw new NotImplementedException();
        }

        public bool IsGuildQueued(int guildId)
        {
            throw new NotImplementedException();
        }

        public bool IsMarriageQueued(int weddingId)
        {
            throw new NotImplementedException();
        }

        public void PutGuildQueued(int guildId)
        {
            throw new NotImplementedException();
        }

        public void PutMarriageQueued(int weddingId, bool isCathedral, bool isPremium, int groomId, int bridgeId)
        {
            throw new NotImplementedException();
        }

        public Task<int> RegisterServer(IWorldChannel server)
        {
            throw new NotImplementedException();
        }

        public void RemoveGuildQueued(int guildId)
        {
            throw new NotImplementedException();
        }

        public KeyValuePair<bool, HashSet<int>> RemoveMarriageQueued(int marriageId)
        {
            throw new NotImplementedException();
        }

        public void ResetDisabledServerMessages()
        {
            throw new NotImplementedException();
        }

        public void SendWorldConfig(WorldConfigPatch updatePatch)
        {
            throw new NotImplementedException();
        }
    }
}
