using Application.Core.Datas;
using Application.Core.Game.Relation;
using Application.Core.Game.TheWorld;
using Application.Core.model;
using Application.Core.ServerTransports;
using Application.Shared.Configs;
using Application.Shared.MapObjects;
using Application.Shared.Net;
using System.Net;

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

        public Task<bool> RemoveServer(IWorldChannel server)
        {
            throw new NotImplementedException();
        }

        public void BroadcastMessage(Packet p)
        {
            throw new NotImplementedException();
        }

        public void BroadcastGMMessage(Packet p)
        {
            throw new NotImplementedException();
        }

        public ITeam CreateTeam(int playerId)
        {
            throw new NotImplementedException();
        }

        public void SetPlayerNpcMapPodiumData(int mapId, int podumData)
        {
            throw new NotImplementedException();
        }

        public int GetPlayerNpcMapPodiumData(int mapId)
        {
            throw new NotImplementedException();
        }

        public void SetPlayerNpcMapStep(int mapId, int step)
        {
            throw new NotImplementedException();
        }

        public int GetPlayerNpcMapStep(int mapId)
        {
            throw new NotImplementedException();
        }

        public void RequestRemovePlayerNpc(int mapId, IEnumerable<int> playerNpcObjectId)
        {
            throw new NotImplementedException();
        }

        public void SendTimer(int seconds)
        {
            throw new NotImplementedException();
        }

        public void RemoveTimer()
        {
            throw new NotImplementedException();
        }

        public List<OwlSearchResult> OwlSearch(int itemId)
        {
            throw new NotImplementedException();
        }

        public PlayerShopDto? SendOwlWarp(int mapId, int ownerId, int searchItem)
        {
            throw new NotImplementedException();
        }

        public int? FindPlayerShopChannel(int ownerId)
        {
            throw new NotImplementedException();
        }

        public void SendAccountLogout(int accountId)
        {
            throw new NotImplementedException();
        }

        public IPEndPoint GetChannelEndPoint(int channel)
        {
            throw new NotImplementedException();
        }

        public void NotifyPartner(int id)
        {
            throw new NotImplementedException();
        }

        public void UpdateAccountState(int accId, sbyte state)
        {
            throw new NotImplementedException();
        }

        public void SetCharacteridInTransition(string v, int cid)
        {
            throw new NotImplementedException();
        }

        public bool HasCharacteridInTransition(string clientSession)
        {
            throw new NotImplementedException();
        }

        public bool WarpPlayer(string name, int? channel, int mapId, int? portal)
        {
            throw new NotImplementedException();
        }

        public string LoadExpeditionInfo()
        {
            throw new NotImplementedException();
        }

        public void ChangePlayerAllianceRank(int targetCharacterId, bool isRaise)
        {
            throw new NotImplementedException();
        }

        public CharacterValueObject? GetPlayerData(string clientSession, int cid)
        {
            throw new NotImplementedException();
        }

        public int GetAccountCharacterCount(int accId)
        {
            throw new NotImplementedException();
        }

        public bool CheckCharacterName(string name)
        {
            throw new NotImplementedException();
        }
    }
}
