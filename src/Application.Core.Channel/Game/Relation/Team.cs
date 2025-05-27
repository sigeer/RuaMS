using net.server.world;
using server.maps;

namespace Application.Core.Game.Relation
{
    public class Team : ITeam
    {
        private int id;
        private int leaderId;
        private List<Player> members = new();
        private List<Player> pqMembers = new List<Player>();

        private Dictionary<int, int> histMembers = new();
        private int nextEntry = 0;

        private Dictionary<int, Door> doors = new();

        private object lockObj = new object();

        public Team(int id, Player chrfor)
        {
            this.leaderId = chrfor.getId();
            this.id = id;
        }

        public bool containsMembers(Player member)
        {
            Monitor.Enter(lockObj);
            try
            {
                return members.Contains(member);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public void addMember(Player member)
        {
            Monitor.Enter(lockObj);
            try
            {
                histMembers.AddOrUpdate(member.getId(), nextEntry);
                nextEntry++;

                members.Add(member);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public void removeMember(Player member)
        {
            Monitor.Enter(lockObj);
            try
            {
                histMembers.Remove(member.getId());

                members.Remove(member);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public void setLeader(Player victim)
        {
            this.leaderId = victim.getId();
        }

        public void updateMember(Player member)
        {
            Monitor.Enter(lockObj);
            try
            {
                for (int i = 0; i < members.Count; i++)
                {
                    if (members.get(i).getId() == member.getId())
                    {
                        members[i] = member;
                    }
                }
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public Player? getMemberById(int id)
        {
            Monitor.Enter(lockObj);
            try
            {
                return members.FirstOrDefault(x => x.getId() == id);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public ICollection<Player> getMembers()
        {
            Monitor.Enter(lockObj);
            try
            {
                return members.ToList();
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public List<Player> getPartyMembersOnline()
        {
            Monitor.Enter(lockObj);
            try
            {
                return members.Where(x => x.IsOnlined).ToList();
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        // used whenever entering PQs: will draw every party member that can attempt a target PQ while ingnoring those unfit.
        public ICollection<Player> getEligibleMembers()
        {
            return pqMembers.ToList();
        }

        public void setEligibleMembers(List<Player> eliParty)
        {
            pqMembers = eliParty;
        }

        public int getId()
        {
            return id;
        }

        public void setId(int id)
        {
            this.id = id;
        }

        public int getLeaderId()
        {
            return leaderId;
        }

        public Player getLeader()
        {
            Monitor.Enter(lockObj);
            try
            {
                return members.FirstOrDefault(x => x.getId() == leaderId) ?? throw new BusinessException();
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public List<int> getMembersSortedByHistory()
        {
            List<KeyValuePair<int, int>> histList;

            Monitor.Enter(lockObj);
            try
            {
                histList = new(histMembers);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }

            histList.Sort((o1, o2) => (o1.Value).CompareTo(o2.Value));

            List<int> histSort = new();
            foreach (var e in histList)
            {
                histSort.Add(e.Key);
            }

            return histSort;
        }

        public sbyte getPartyDoor(int cid)
        {
            List<int> histList = getMembersSortedByHistory();
            sbyte slot = 0;
            foreach (int e in histList)
            {
                if (e == cid)
                {
                    break;
                }
                slot++;
            }

            return slot;
        }

        public void addDoor(int owner, Door door)
        {
            Monitor.Enter(lockObj);
            try
            {
                this.doors.AddOrUpdate(owner, door);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public void removeDoor(int owner)
        {
            Monitor.Enter(lockObj);
            try
            {
                this.doors.Remove(owner);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public Dictionary<int, Door> getDoors()
        {
            Monitor.Enter(lockObj);
            try
            {
                return new Dictionary<int, Door>(doors);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public void assignNewLeader(ChannelClient c)
        {
            var world = c.getWorldServer();
            Player? newLeadr = null;

            Monitor.Enter(lockObj);
            try
            {
                foreach (Player mpc in members)
                {
                    if (mpc.getId() != leaderId && (newLeadr == null || newLeadr.getLevel() < mpc.getLevel()))
                    {
                        newLeadr = mpc;
                    }
                }
            }
            finally
            {
                Monitor.Exit(lockObj);
            }

            if (newLeadr != null)
            {
                world.updateParty(this.getId(), PartyOperation.CHANGE_LEADER, newLeadr);
            }
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + id;
            return result;
        }

        public Player? getMemberByPos(int pos)
        {
            return members.ElementAtOrDefault(pos);
        }

        public override bool Equals(object? obj)
        {
            return obj is Team other && other.getId() == id;
        }
    }
}
