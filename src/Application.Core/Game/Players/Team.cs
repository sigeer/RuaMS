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
        public ITeam? getParty()
        {
            Monitor.Enter(prtLock);
            try
            {
                return TeamModel;
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
        }

        public int getPartyId()
        {
            Monitor.Enter(prtLock);
            try
            {
                return TeamModel?.getId() ?? -1;
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
        }

        public List<IPlayer> getPartyMembersOnline()
        {
            Monitor.Enter(prtLock);
            try
            {
                return TeamModel?.getPartyMembersOnline() ?? new List<IPlayer>();
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
        }

        public List<IPlayer> getPartyMembersOnSameMap()
        {
            List<IPlayer> list = new();
            int thisMapHash = this.MapModel.GetHashCode();

            Monitor.Enter(prtLock);
            try
            {
                if (TeamModel != null)
                {
                    foreach (var chr in TeamModel.getMembers())
                    {
                        var chrMap = chr.getMap();
                        // 用hashcode判断地图是否相同？ -- 同一频道、同一mapid
                        if (chrMap != null && chrMap.GetHashCode() == thisMapHash && chr.isLoggedinWorld())
                        {
                            list.Add(chr);
                        }
                    }
                }
            }
            finally
            {
                Monitor.Exit(prtLock);
            }

            return list;
        }

        public bool isPartyMember(IPlayer chr)
        {
            return isPartyMember(chr.Id);
        }

        public bool isPartyMember(int cid)
        {
            Monitor.Enter(prtLock);
            try
            {
                return TeamModel?.getMemberById(cid) != null;
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
