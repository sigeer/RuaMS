using Application.Core.Game.Relation;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public void setParty(Team? p)
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


        public bool isLeader()
        {
            return isPartyLeader();
        }
        public bool isPartyLeader()
        {
            Monitor.Enter(prtLock);
            try
            {
                return TeamModel?.getLeaderId() == getId();
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
        }


        public Team? getParty()
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
                if (TeamModel != null)
                {
                    return TeamModel.GetChannelMembers(Client.CurrentServer).Where(x => x.IsOnlined).ToList();
                }
                return new List<IPlayer>();
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
        }

        public List<IPlayer> getPartyMembersOnSameMap()
        {
            List<IPlayer> list = new();
            Monitor.Enter(prtLock);
            try
            {
                if (TeamModel != null)
                {
                    int thisMapHash = this.MapModel.GetHashCode();
                    foreach (var chr in TeamModel.GetChannelMembers(Client.CurrentServer))
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
            return isPartyMember(chr.getId());
        }

        public bool isPartyMember(int cid)
        {
            Monitor.Enter(prtLock);
            try
            {
                if (TeamModel != null)
                {
                    return TeamModel.containsMembers(cid);
                }
            }
            finally
            {
                Monitor.Exit(prtLock);
            }

            return false;
        }

        public bool leaveParty()
        {
            Team? party;
            bool partyLeader;

            Monitor.Enter(prtLock);
            try
            {
                party = getParty();
                partyLeader = isPartyLeader();
            }
            finally
            {
                Monitor.Exit(prtLock);
            }

            if (party != null)
            {
                Client.CurrentServerContainer.TeamManager.LeaveParty(this);

                return true;
            }
            else
            {
                return false;
            }
        }

        public void updatePartyMemberHP()
        {
            Monitor.Enter(prtLock);
            try
            {
                if (TeamModel != null)
                {
                    foreach (var player in this.getPartyMembersOnSameMap())
                    {
                        player.sendPacket(PacketCreator.updatePartyMemberHP(getId(), HP, ActualMaxHP));
                    }
                }
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
        }


        public void receivePartyMemberHP()
        {
            Monitor.Enter(prtLock);
            try
            {
                if (TeamModel != null)
                {
                    foreach (var player in this.getPartyMembersOnSameMap())
                    {
                        sendPacket(PacketCreator.updatePartyMemberHP(player.getId(), player.HP, player.ActualMaxHP));
                    }
                }
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
        }
    }
}
