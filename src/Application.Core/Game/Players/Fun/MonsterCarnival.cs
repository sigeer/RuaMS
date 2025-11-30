using Application.Core.Game.GameEvents.CPQ;
using server.partyquest;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public MonsterCarnivalTeam? MCTeam => TeamModel?.MCTeam;
        public int TotalCP { get; private set; }
        public int AvailableCP { get; private set; }

        public void setMonsterCarnival(MonsterCarnival? monsterCarnival)
        {
            
        }
    }
}
