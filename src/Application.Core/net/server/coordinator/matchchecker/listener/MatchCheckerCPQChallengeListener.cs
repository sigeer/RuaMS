using client;
using constants.String;
using net.server.coordinator.matchchecker;
using net.server.world;
using scripting.npc;

namespace Application.Core.net.server.coordinator.matchchecker.listener
{
    public class MatchCheckerCPQChallengeListener : AbstractMatchCheckerListener
    {
        private static Character? getChallenger(int leaderid, HashSet<Character> matchPlayers)
        {
            return matchPlayers.FirstOrDefault(x => x.getId() == leaderid);
        }
        public override void onMatchCreated(Character leader, HashSet<Character> nonLeaderMatchPlayers, string message)
        {
            NPCConversationManager cm = leader.getClient().getCM();
            int npcid = cm.getNpc();

            Character? ldr = nonLeaderMatchPlayers.FirstOrDefault();

            Character chr = leader;

            List<PartyCharacter> chrMembers = new();
            foreach (PartyCharacter mpc in chr.getParty().getMembers())
            {
                if (mpc.isOnline())
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

        public override void onMatchAccepted(int leaderid, HashSet<Character> matchPlayers, string message)
        {
            var chr = getChallenger(leaderid, matchPlayers)!;

            Character ldr = matchPlayers.FirstOrDefault(x => x != chr)!;

            if (message == ("cpq1"))
            {
                ldr.getClient().getCM().startCPQ(chr, ldr.getMapId() + 1);
            }
            else
            {
                ldr.getClient().getCM().startCPQ2(chr, ldr.getMapId() + 1);
            }

            ldr.getParty().setEnemy(chr.getParty());
            chr.getParty().setEnemy(ldr.getParty());
            chr.setChallenged(false);
        }

        public override void onMatchDeclined(int leaderid, HashSet<Character> matchPlayers, string message)
        {
            var chr = getChallenger(leaderid, matchPlayers);
            chr.dropMessage(5, LanguageConstants.getMessage(chr, LanguageConstants.CPQChallengeRoomDenied));
        }

        public override void onMatchDismissed(int leaderid, HashSet<Character> matchPlayers, string message) { }
    }
}
