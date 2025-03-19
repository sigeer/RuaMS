using server.partyquest;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private MonsterCarnival? monsterCarnival;
        public MonsterCarnivalParty? MCTeam { get; set; }
        public int TotalCP { get; private set; }
        public int AvailableCP { get; private set; }

        public MonsterCarnival? getMonsterCarnival()
        {
            return monsterCarnival;
        }

        public void setMonsterCarnival(MonsterCarnival? monsterCarnival)
        {
            this.monsterCarnival = monsterCarnival;
        }
    }
}
