using Application.Core.Game.Relation;
using Application.Core.Login.Models;
using Application.EF.Entities;
using Application.Shared.Configs;
using Application.Shared.Constants.Item;
using Application.Shared.Constants.Job;
using Application.Shared.Servers;
using Application.Shared.Team;
using Dto;
using net.server;
using Org.BouncyCastle.Asn1.X509;
using System.Xml.Linq;
using tools;

namespace Application.Core.Login
{
    public class MasterServerTransport : IServerTransport
    {
        readonly MasterServer _server;
        public MasterServerTransport(MasterServer masterServer)
        {
            this._server = masterServer;
        }

        public void BroadcastMessage(Packet p)
        {
            var world = Server.getInstance().getWorld(0);
            foreach (var ch in world.Channels)
            {
                ch.broadcastPacket(p);
            }
        }

        public void BroadcastWorldGMPacket(Packet packet)
        {
            var world = Server.getInstance().getWorld(0);
            foreach (var ch in world.Channels)
            {
                ch.broadcastGMPacket(packet);
            }
        }

        public Dto.CreateCharResponseDto CreatePlayer(Dto.CreateCharRequestDto request)
        {
            var world = Server.getInstance().getWorld(0);
            var channel = world.Channels[0];
            var statusCode = channel.Service.CreatePlayer(
                request.Type, request.AccountId,
                request.Name, request.Face, request.Hair, request.SkinColor, request.Top, request.Bottom, request.Shoes, request.Weapon, request.Gender);
            return new Dto.CreateCharResponseDto() { Code = statusCode };
        }

        public CoupleIdPair? GetAllWeddingCoupleForGuest(int guestId, bool cathedral)
        {
            var world = Server.getInstance().getWorld(0);
            foreach (var ch in world.Channels)
            {
                var p = ch.WeddingInstance.GetWeddingCoupleForGuest(guestId, cathedral);
                if (p != null)
                {
                    return p;
                }
            }
            return null;
        }

        public int GetAllWeddingReservationStatus(IEnumerable<int> possibleWeddings, bool cathedral)
        {
            var world = Server.getInstance().getWorld(0);
            int selectedPw = -1;
            int selectedPos = int.MaxValue;

            foreach (int pw in possibleWeddings)
            {
                foreach (var ch in world.Channels)
                {
                    int pos = ch.WeddingInstance.GetWeddingReservationStatus(pw, cathedral);
                    if (pos != -1)
                    {
                        if (pos < selectedPos)
                        {
                            selectedPos = pos;
                            selectedPw = pw;
                            break;
                        }
                    }
                }
            }

            return selectedPw;
        }

        public void SendChannelPlayerPacket(int channel, int playerId, Packet packet)
        {
            var world = Server.getInstance().getWorld(0);
            var chr = world.Channels[channel - 1].getPlayerStorage().getCharacterById(playerId);
            if (chr != null)
            {
                chr.Client.sendPacket(packet);
            }
        }

        public void SendDueyNotification(int channel, int id, string senderName, bool dueyType)
        {
            var world = Server.getInstance().getWorld(0);
            var chr = world.Channels[channel - 1].getPlayerStorage().getCharacterById(id);
            if (chr != null)
            {
                chr.Client.sendPacket(PacketCreator.sendDueyParcelReceived(senderName, dueyType));
            }
        }

        public void SendNotes(int channel, int id, Dto.NoteDto[] notes)
        {
            var world = Server.getInstance().getWorld(0);
            world.Channels[channel - 1].Service.SendNoteMessage(id, notes);
        }

        public void SendServerMessage(IEnumerable<int> playerIdList)
        {
            if (playerIdList.Count() > 0)
            {
                var world = Server.getInstance().getWorld(0);
                foreach (int chrid in playerIdList)
                {
                    var chr = world.Players.getCharacterById(chrid);

                    if (chr != null && chr.isLoggedinWorld())
                    {
                        chr.sendPacket(PacketCommon.serverMessage(_server.ServerMessage));
                    }
                }
            }
        }


        public void SendMultiChat(int type, string nameFrom, PlayerChannelPair[] teamMember, string chatText)
        {
            var world = Server.getInstance().getWorld(0);
            var data = teamMember.GroupBy(x => x.Channel).ToDictionary(x => x.Key, x => x.Select(y => y.PlayerId).ToArray());
            foreach (var item in data)
            {
                var ch = _server.GetChannelServer(item.Key);
                ch.SendMultiChat(type, nameFrom, item.Value, chatText);
            }
        }

        public void SendUpdateCouponRates(Config.CouponConfig config)
        {
            foreach (var server in _server.ChannelServerList)
            {
                server.Value.UpdateCouponConfig(config);
            }
        }

        public void SendWorldConfig(WorldConfigPatch patch)
        {
            var world = Server.getInstance().getWorld(0);
            foreach (var ch in world.Channels)
            {
                ch.UpdateWorldConfig(patch);
            }
        }

        public bool WrapPlayer(string name, int? channel, int mapId, int? portal)
        {
            var world = Server.getInstance().getWorld(0);
            foreach (var ch in world.getChannels())
            {
                var target = ch.Players.getCharacterByName(name);
                if (target != null)
                {
                    if (portal != null)
                        target.changeMap(mapId, portal.Value);
                    else
                        target.changeMap(mapId);

                    if (channel != null && target.Channel != channel)
                    {
                        target.Client.ChangeChannel(channel.Value);
                    }
                    return true;
                }
            }
            return false;
        }


        internal void BroadcastJobChanged(int type, IDictionary<int, int[]> targets, string name, int jobId)
        {
            foreach (var item in targets)
            {
                var ch = _server.GetChannelServer(item.Key);
                ch.BroadcastJobChanged(type, item.Value, name, jobId);
            }
        }

        internal void BroadcastLevelChanged(int type, IDictionary<int, int[]> targets, string name, int level)
        {
            foreach (var item in targets)
            {
                var ch = _server.GetChannelServer(item.Key);
                ch.BroadcastLevelChanged(type, item.Value, name, level);
            }

        }

        internal void BroadcastTeamUpdate(string exceptServer, int teamId, PartyOperation operation, TeamMemberDto target)
        {
            foreach (var server in _server.ChannelServerList)
            {
                if (server.Key == exceptServer)
                    continue;

                server.Value.SendTeamUpdate(teamId, operation, target);
            }
        }

        internal void BroadcastGuildUpdate(string exceptServer, UpdateGuildResponse response)
        {
            foreach (var server in _server.ChannelServerList)
            {
                if (server.Key == exceptServer)
                    continue;

                server.Value.SendGuildUpdate(response);
            }
        }


        internal void BroadcastAllianceUpdate(string exceptServer, UpdateAllianceResponse response)
        {
            foreach (var server in _server.ChannelServerList)
            {
                if (server.Key == exceptServer)
                    continue;

                server.Value.SendAllianceUpdate(response);
            }
        }
    }
}
