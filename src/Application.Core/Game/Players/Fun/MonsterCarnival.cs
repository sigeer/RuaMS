using Application.Core.Game.GameEvents.CPQ;
using server.partyquest;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public MonsterCarnivalTeam? MCTeam { get; set; }
        public int TotalCP { get; private set; }
        public int AvailableCP { get; private set; }

        public void setMonsterCarnival(MonsterCarnival? monsterCarnival)
        {
            
        }

        public void ClearMC()
        {
            MCTeam = null;
            resetCP();
            setTeam(-1);
            setEventInstance(null);
        }
    }
}
