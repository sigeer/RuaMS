using server.partyquest;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private MonsterCarnival? monsterCarnival;
        private MonsterCarnivalParty? monsterCarnivalParty = null;

        public MonsterCarnival? getMonsterCarnival()
        {
            return monsterCarnival;
        }

        public void setMonsterCarnival(MonsterCarnival? monsterCarnival)
        {
            this.monsterCarnival = monsterCarnival;
        }



        public MonsterCarnivalParty? getMonsterCarnivalParty()
        {
            return this.monsterCarnivalParty;
        }

        public void setMonsterCarnivalParty(MonsterCarnivalParty? mcp)
        {
            this.monsterCarnivalParty = mcp;
        }
    }
}
