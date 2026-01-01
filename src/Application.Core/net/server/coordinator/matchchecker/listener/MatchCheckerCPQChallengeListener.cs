using constants.String;
using net.server.coordinator.matchchecker;

namespace Application.Core.net.server.coordinator.matchchecker.listener
{
    public class MatchCheckerCPQChallengeListener : AbstractMatchCheckerListener
    {
        private static IPlayer? getChallenger(int leaderid, HashSet<IPlayer> matchPlayers)
        {
            return matchPlayers.FirstOrDefault(x => x.getId() == leaderid);
        }
        public override Task onMatchCreated(IPlayer leader, HashSet<IPlayer> nonLeaderMatchPlayers, string message)
        {
            var cm = leader.getClient().NPCConversationManager;
            int npcid = cm.getNpc();

            var ldr = nonLeaderMatchPlayers.FirstOrDefault();

            var chr = leader;

            List<IPlayer> chrMembers = new();
            foreach (IPlayer mpc in chr.getParty().GetChannelMembers(leader.Client.CurrentServer))
            {
                if (mpc.IsOnlined)
                {
                    chrMembers.Add(mpc);
                }
            }

            if (message == ("cpq1"))
            {
                leader.getClient().CurrentServer.NPCScriptManager.start("cpqchallenge", ldr.getClient(), npcid, chrMembers);
            }
            else
            {
                leader.getClient().CurrentServer.NPCScriptManager.start("cpqchallenge2", ldr.getClient(), npcid, chrMembers);
            }

            cm.sendOk(LanguageConstants.getMessage(chr, LanguageConstants.CPQChallengeRoomSent));
            return Task.CompletedTask;
        }

        public override Task onMatchAccepted(int leaderid, HashSet<IPlayer> matchPlayers, string message)
        {
            var chr = getChallenger(leaderid, matchPlayers)!;

            var ldr = matchPlayers.FirstOrDefault(x => x != chr)!;

            if (message == ("cpq1"))
            {
                ldr.getClient().NPCConversationManager?.startCPQ(chr, ldr.getMapId() + 1);
            }
            else
            {
                ldr.getClient().NPCConversationManager?.startCPQ2(chr, ldr.getMapId() + 1);
            }

            chr.setChallenged(false);
            return Task.CompletedTask;
        }

        public override Task onMatchDeclined(int leaderid, HashSet<IPlayer> matchPlayers, string message)
        {
            var chr = getChallenger(leaderid, matchPlayers);
            chr.dropMessage(5, LanguageConstants.getMessage(chr, LanguageConstants.CPQChallengeRoomDenied));
            return Task.CompletedTask;
        }

        public override Task onMatchDismissed(int leaderid, HashSet<IPlayer> matchPlayers, string message) {
            return Task.CompletedTask;
        }
    }
}
