using Application.Core.Channel.ServerData;
using constants.game;
using net.server.coordinator.matchchecker;
using net.server.guild;

namespace Application.Core.net.server.coordinator.matchchecker.listener
{
    public class MatchCheckerGuildCreationListener : AbstractMatchCheckerListener
    {
        GuildManager _guildManager;
        TeamManager _teamManager;

        public MatchCheckerGuildCreationListener(GuildManager guildManager, TeamManager teamManager)
        {
            _guildManager = guildManager;
            _teamManager = teamManager;
        }

        private static void broadcastGuildCreationDismiss(HashSet<IPlayer> nonLeaderMatchPlayers)
        {
            foreach (var chr in nonLeaderMatchPlayers)
            {
                if (chr.isLoggedinWorld())
                {
                    chr.sendPacket(GuildPackets.genericGuildMessage(0x26));
                }
            }
        }
        public override void onMatchCreated(IPlayer leader, HashSet<IPlayer> nonLeaderMatchPlayers, string message)
        {
            Packet createGuildPacket = GuildPackets.createGuildMessage(leader.getName(), message);

            foreach (var chr in nonLeaderMatchPlayers)
            {
                if (chr.isLoggedinWorld())
                {
                    chr.sendPacket(createGuildPacket);
                }
            }
        }

        public override void onMatchAccepted(int leaderid, HashSet<IPlayer> matchPlayers, string message)
        {
            _guildManager.CreateGuild(message, leaderid, matchPlayers, () =>
            {
                broadcastGuildCreationDismiss(matchPlayers);
            });
        }

        public override void onMatchDeclined(int leaderid, HashSet<IPlayer> matchPlayers, string message)
        {
            foreach (var chr in matchPlayers)
            {
                if (chr.getId() == leaderid && chr.getClient() != null)
                {
                    _teamManager.LeaveParty(chr);
                }

                if (chr.isLoggedinWorld())
                {
                    chr.sendPacket(GuildPackets.genericGuildMessage(0x26));
                }
            }
        }

        public override void onMatchDismissed(int leaderid, HashSet<IPlayer> matchPlayers, string message)
        {

            var leader = matchPlayers.FirstOrDefault(x => x.getId() == leaderid);

            string msg;
            if (leader != null && leader.getParty() == null)
            {
                msg = "The Guild creation has been dismissed since the leader left the founding party.";
            }
            else
            {
                msg = "The Guild creation has been dismissed since a member was already in a party when they answered.";
            }

            foreach (var chr in matchPlayers)
            {
                if (chr.getId() == leaderid && chr.getClient() != null)
                {
                    _teamManager.LeaveParty(chr);
                }

                if (chr.isLoggedinWorld())
                {
                    chr.message(msg);
                    chr.sendPacket(GuildPackets.genericGuildMessage(0x26));
                }
            }
        }
    }
}
