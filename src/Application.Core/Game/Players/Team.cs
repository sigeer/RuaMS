using Application.Core.Game.Relation;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public void setParty(ITeam? p)
        {
            Monitor.Enter(prtLock);
            try
            {
                if (p == null)
                {
                    doorSlot = -1;

                    TeamModel = null;
                }
                else
                {
                    TeamModel = p;
                }
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
        }

        public void updatePartyMemberHP()
        {
            Monitor.Enter(prtLock);
            try
            {
                updatePartyMemberHPInternal();
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
        }

        private void updatePartyMemberHPInternal()
        {
            if (TeamModel != null)
            {
                int curmaxhp = getCurrentMaxHp();
                int curhp = getHp();
                foreach (IPlayer partychar in this.getPartyMembersOnSameMap())
                {
                    partychar.sendPacket(PacketCreator.updatePartyMemberHP(getId(), curhp, curmaxhp));
                }
            }
        }
    }
}
