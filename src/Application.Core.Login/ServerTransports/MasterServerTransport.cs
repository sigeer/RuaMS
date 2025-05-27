using Application.Core.model;
using Application.Core.Servers;
using Application.Core.ServerTransports;
using Application.Shared.Configs;
using Application.Shared.Dto;
using Application.Shared.Net;
using net.packet.outs;
using net.server;
using tools;

namespace Application.Core.Login
{
    public class MasterServerTransport : IMasterServerTransport
    {
        readonly IMasterServer _server;

        public MasterServerTransport(IMasterServer masterServer)
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

        public void SendNotes(int channel, int id, NoteDto[] notes)
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
                        chr.sendPacket(PacketCreator.serverMessage(_server.ServerMessage));
                    }
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
    }
}
