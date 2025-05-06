using Application.Core.model;
using constants.id;
using net.server;
using System;

namespace Application.Core.Game.TheWorld
{
    public class LocalChannelServerTransport : IChannelSeverTransport
    {
        public void BroadcastGMMessage(Packet p)
        {
            Server.getInstance().broadcastGMMessage(0, p);
        }

        public void BroadcastMessage(Packet p)
        {
            Server.getInstance().broadcastMessage(0, p);
        }

        public void DropMessage(int type, string message)
        {
            Server.getInstance().getWorld(0).dropMessage(type, message);
        }

        public CoupleIdPair? GetMarriageQueuedCouple(int weddingId)
        {
            return Server.getInstance().getWorld(0).WeddingInstance.GetMarriageQueuedCouple(weddingId);
        }

        public CoupleIdPair? GetRelationshipCouple(int cathedralId)
        {
            return Server.getInstance().getWorld(0).WeddingInstance.GetRelationshipCouple(cathedralId);
        }

        public bool IsMarriageQueued(int weddingId)
        {
            return Server.getInstance().getWorld(0).WeddingInstance.IsMarriageQueued(weddingId);
        }

        public void PutMarriageQueued(int weddingId, bool isCathedral, bool isPremium, int groomId, int bridgeId)
        {
            Server.getInstance().getWorld(0).WeddingInstance.PutMarriageQueued(weddingId, isCathedral, isPremium, groomId, bridgeId);
        }

        public KeyValuePair<bool, HashSet<int>> RemoveMarriageQueued(int marriageId)
        {
            return Server.getInstance().getWorld(0).WeddingInstance.RemoveMarriageQueued(marriageId);
        }


        public long GetServerCurrentTime()
        {
            return Server.getInstance().getCurrentTime();
        }

        public int GetServerCurrentTimestamp()
        {
            return Server.getInstance().getCurrentTimestamp();
        }

        public DateTimeOffset GetServerUpTime()
        {
            return Server.uptime;
        }


        public Task<int> RegisterChannel(IWorldChannel worldChannel)
        {
            return Task.FromResult(Server.getInstance().AddChannel(new InternalWorldChannel(worldChannel)));
        }

        public Task UnRegisterChannel(string id)
        {
            return Task.FromResult(Server.getInstance().RemoveChannel(id));
        }

        public float GetWorldMobRate()
        {
            return Server.getInstance().getWorld(0).MobRate;
        }

        public int GetTransportationTime(double travelTime)
        {
            return Server.getInstance().getWorld(0).getTransportationTime(travelTime);
        }

        public void DisconnectPlayers(IEnumerable<int> playerIdList)
        {
            var wserv = Server.getInstance().getWorld(0);
            foreach (int cid in playerIdList)
            {
                var chr = wserv.Players.getCharacterById(cid);
                if (chr != null && chr.IsOnlined)
                {
                    chr.getClient().forceDisconnect();
                }
            }
        }

        public void RemoveGuildQueued(int guildId)
        {
            Server.getInstance().getWorld(0).removeGuildQueued(guildId);
        }

        public bool IsGuildQueued(int guildId)
        {
            return Server.getInstance().getWorld(0).isGuildQueued(guildId);
        }

        public void PutGuildQueued(int guildId)
        {
            Server.getInstance().getWorld(0).putGuildQueued(guildId);
        }

        public void ResetDisabledServerMessages()
        {
            foreach (var ch in Server.getInstance().getAllChannels())
            {
                ch.ServerMessageController.resetDisabledServerMessages();
            }
        }

        public int GetPlayerNpcMapStep(int mapid)
        {
            return Server.getInstance().getWorld(0).getPlayerNpcMapStep(mapid);
        }

        public void SetPlayerNpcMapStep(int mapid, int step)
        {
            Server.getInstance().getWorld(0).setPlayerNpcMapStep(mapid, step);
        }

        public int GetPlayerNpcMapPodiumData(int mapid)
        {
            return Server.getInstance().getWorld(0).getPlayerNpcMapPodiumData(mapid);
        }

        public void SetPlayerNpcMapPodiumData(int mapid, int podium)
        {
            Server.getInstance().getWorld(0).setPlayerNpcMapPodiumData(mapid, podium);
        }

        public List<int> FilterPlayerId(IEnumerable<int> playerIdList, Func<IPlayer, bool> func)
        {
            return AllPlayerStorage.GetPlayersByIds(playerIdList).Where(x => func(x)).Select(x => x.Id).ToList();
        }

        public void BroadcastMessage(IEnumerable<int> playerIdList, Packet p)
        {
            var players = AllPlayerStorage.GetPlayersByIds(playerIdList);
            foreach (var v in players)
            {
                v.sendPacket(p);
            }
        }

        public void ResetFamilyDailyReps()
        {
            foreach (var family in Server.getInstance().getWorld(0).getFamilies())
            {
                family.resetDailyReps();
            }
        }
    }
}
