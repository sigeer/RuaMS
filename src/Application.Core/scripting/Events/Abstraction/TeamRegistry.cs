using Application.Core.Game.GameEvents.CPQ;

namespace Application.Core.scripting.Events.Abstraction
{
    public class TeamRegistry
    {
        public int Team { get; }
        public List<Player> EligibleMembers { get; }
        public MonsterCarnivalTeam? MCTeam { get; set; }
        public TeamRegistry(int team, List<Player> list, MonsterCarnivalTeam? mCTeam)
        {
            Team = team;
            EligibleMembers = list;
            MCTeam = mCTeam;
        }
    }
}
