using Application.Core.Channel.ServerData;
using net.server.coordinator.matchchecker;
using net.server.guild;
using System.Threading.Tasks;

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

        private static Task broadcastGuildCreationDismiss(HashSet<Player> nonLeaderMatchPlayers)
        {
            foreach (var chr in nonLeaderMatchPlayers)
            {
                if (chr.isLoggedinWorld())
                {
                    chr.sendPacket(GuildPackets.genericGuildMessage(0x26));
                }
            }
            return Task.CompletedTask;
        }
        public override Task onMatchCreated(Player leader, HashSet<Player> nonLeaderMatchPlayers, string message)
        {
            Packet createGuildPacket = GuildPackets.createGuildMessage(leader.getName(), message);

            foreach (var chr in nonLeaderMatchPlayers)
            {
                if (chr.isLoggedinWorld())
                {
                    chr.sendPacket(createGuildPacket);
                }
            }
            return Task.CompletedTask;
        }

        public override Task onMatchAccepted(int leaderid, HashSet<Player> matchPlayers, string message)
        {
            _guildManager.CreateGuild(message, leaderid, matchPlayers, () =>
            {
                broadcastGuildCreationDismiss(matchPlayers);
            });
            return Task.CompletedTask;
        }

        public override async Task onMatchDeclined(int leaderid, HashSet<Player> matchPlayers, string message)
        {
            foreach (var chr in matchPlayers)
            {
                if (chr.getId() == leaderid && chr.getClient() != null)
                {
                    await _teamManager.LeaveParty(chr);
                }

                if (chr.isLoggedinWorld())
                {
                    chr.sendPacket(GuildPackets.genericGuildMessage(0x26));
                }
            }
        }

        public override async Task onMatchDismissed(int leaderid, HashSet<Player> matchPlayers, string message)
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
                    await _teamManager.LeaveParty(chr);
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
