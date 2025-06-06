using Application.Core.Login.Models;
using Application.Shared.Configs;
using Application.Shared.Servers;
using Application.Shared.Team;
using Dto;
using net.server;
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


        public void SendTeamChat(string nameFrom, PlayerChannelPair[] teamMember, string chatText)
        {
            var world = Server.getInstance().getWorld(0);
            var data = teamMember.GroupBy(x => x.Channel).ToDictionary(x => x.Key, x => x.Select(y => y.PlayerId).ToArray());
            foreach (var item in data)
            {
                world.Channels[item.Key - 1].Service.SendTeamChat(nameFrom, item.Value, chatText);
            }
        }

        public void SendUpdateCouponRates(Config.CouponConfig config)
        {
            var world = Server.getInstance().getWorld(0);
            foreach (var ch in world.Channels)
            {
                ch.UpdateCouponConfig(config);

                foreach (var chr in ch.getPlayerStorage().getAllCharacters())
                {
                    if (!chr.isLoggedin())
                    {
                        continue;
                    }

                    chr.updateCouponRates();
                }
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

        internal void SendTeamUpdate(int exceptChannel, int teamId, PartyOperation operation, TeamMemberDto target)
        {
            for (int i = 0; i < _server.ChannelServerList.Count; i++)
            {
                if (i == exceptChannel - 1)
                    continue;

                var ch = _server.ChannelServerList[i];
                ch.SendTeamUpdate(teamId, operation, target);
            }
        }
    }
}
