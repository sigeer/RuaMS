using constants.String;
using net.server.coordinator.matchchecker;
using scripting.npc;

namespace Application.Core.net.server.coordinator.matchchecker.listener
{
    public class MatchCheckerCPQChallengeListener : AbstractMatchCheckerListener
    {
        private static IPlayer? getChallenger(int leaderid, HashSet<IPlayer> matchPlayers)
        {
            return matchPlayers.FirstOrDefault(x => x.getId() == leaderid);
        }
        public override void onMatchCreated(IPlayer leader, HashSet<IPlayer> nonLeaderMatchPlayers, string message)
        {
            NPCConversationManager cm = leader.getClient().getCM();
            int npcid = cm.getNpc();

            var ldr = nonLeaderMatchPlayers.FirstOrDefault();

            var chr = leader;

            List<IPlayer> chrMembers = new();
            foreach (IPlayer mpc in chr.getParty().getMembers())
            {
                if (mpc.IsOnlined)
                {
                    chrMembers.Add(mpc);
                }
            }

            if (message == ("cpq1"))
            {
                NPCScriptManager.getInstance().start("cpqchallenge", ldr.getClient(), npcid, chrMembers);
            }
            else
            {
                NPCScriptManager.getInstance().start("cpqchallenge2", ldr.getClient(), npcid, chrMembers);
            }

            cm.sendOk(LanguageConstants.getMessage(chr, LanguageConstants.CPQChallengeRoomSent));
        }

        public override void onMatchAccepted(int leaderid, HashSet<IPlayer> matchPlayers, string message)
        {
            var chr = getChallenger(leaderid, matchPlayers)!;

            var ldr = matchPlayers.FirstOrDefault(x => x != chr)!;

            if (message == ("cpq1"))
            {
                ldr.getClient().getCM()?.startCPQ(chr, ldr.getMapId() + 1);
            }
            else
            {
                ldr.getClient().getCM()?.startCPQ2(chr, ldr.getMapId() + 1);
            }

            ldr.getParty().setEnemy(chr.getParty());
            chr.getParty().setEnemy(ldr.getParty());
            chr.setChallenged(false);
        }

        public override void onMatchDeclined(int leaderid, HashSet<IPlayer> matchPlayers, string message)
        {
            var chr = getChallenger(leaderid, matchPlayers);
            chr.dropMessage(5, LanguageConstants.getMessage(chr, LanguageConstants.CPQChallengeRoomDenied));
        }

        public override void onMatchDismissed(int leaderid, HashSet<IPlayer> matchPlayers, string message) { }
    }
}
