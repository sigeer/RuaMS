using Application.Core.Game.Relation;
using Application.Core.Game.TheWorld;
using Application.Core.model;
using Application.Core.Servers;
using Application.Core.ServerTransports;
using Application.Shared.Configs;
using Application.Shared.Relations;
using Application.Shared.Servers;
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

        public void SyncTeam(ITeamGlobal teamGlobal)
        {
            var world = Server.getInstance().getWorld(0);
            foreach (var ch in world.Channels)
            {
                ch.SyncTeam(teamGlobal);
            }
        }

        public void SendExpelFromParty(int partyId, int expelCid)
        {
            var world = Server.getInstance().getWorld(0);
            foreach (var ch in world.Channels)
            {
                if (ch is IWorldChannelProcessor processor)
                    processor.ProcessExpelFromParty(partyId, expelCid);

            }
        }

        public void UpdateTeamChannelData(int partyId, PartyOperation operation, TeamMember targetMember)
        {
            var world = Server.getInstance().getWorld(0);
            foreach (var ch in world.Channels)
            {
                if (ch is IWorldChannelProcessor processor)
                    processor.ProcessUpdateTeamChannelData(partyId, operation, targetMember);
            }
        }

        public void SendTeamMessage(int teamId, string from, string message)
        {
            var world = Server.getInstance().getWorld(0);
            foreach (var ch in world.Channels)
            {
                if (ch is IWorldChannelProcessor processor)
                    processor.ProcessBroadcastTeamMessage(teamId, from, message);
            }
        }
    }
}
