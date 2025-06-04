using Application.Core.Channel;
using server.maps;

namespace Application.Core.Game.Relation
{
    public class Team
    {
        private int id;
        private int leaderId;
        private Dictionary<int, TeamMember> members = new();
        private List<IPlayer> pqMembers = new List<IPlayer>();

        private Dictionary<int, int> histMembers = new();
        private int nextEntry = 0;

        private Dictionary<int, Door> doors = new();

        private object lockObj = new object();

        public Team(int id, int leaderId)
        {
            this.leaderId = leaderId;
            this.id = id;
        }

        public bool containsMembers(int memberId)
        {
            Monitor.Enter(lockObj);
            try
            {
                return members.ContainsKey(memberId);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public void addMember(TeamMember member)
        {
            Monitor.Enter(lockObj);
            try
            {
                histMembers.AddOrUpdate(member.Id, nextEntry);
                nextEntry++;

                members.TryAdd(member.Id, member);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public void removeMember(int member)
        {
            Monitor.Enter(lockObj);
            try
            {
                histMembers.Remove(member);

                members.Remove(member);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public void setLeader(IPlayer victim)
        {
            this.leaderId = victim.getId();
        }

        public void updateMember(TeamMember member)
        {
            Monitor.Enter(lockObj);
            try
            {
                members[member.Id] = member;
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public IPlayer? getMemberById(WorldChannel currentServer, int id)
        {
            Monitor.Enter(lockObj);
            try
            {
                return GetChannelMembers(currentServer).FirstOrDefault(x => x.getId() == id);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public int GetMemberCount() => members.Count;
        public List<TeamMember> GetTeamMembers() => members.Values.ToList();

        /// <summary>
        /// 当需要获取完整Player时的获取队员，都会要求在同一频道
        /// </summary>
        /// <param name="currentServer"></param>
        /// <returns></returns>
        public ICollection<IPlayer> GetChannelMembers(WorldChannel currentServer)
        {
            Monitor.Enter(lockObj);
            try
            {
                return members.Keys.Select(x => currentServer.Players.getCharacterById(x)).Where(x => x != null).ToList()!;
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        // used whenever entering PQs: will draw every party member that can attempt a target PQ while ingnoring those unfit.
        public ICollection<IPlayer> getEligibleMembers()
        {
            return pqMembers.ToList();
        }

        public void setEligibleMembers(List<IPlayer> eliParty)
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

        public IPlayer? GetChannelLeader(WorldChannel server)
        {
            Monitor.Enter(lockObj);
            try
            {
                return GetChannelMembers(server).FirstOrDefault(x => x.getId() == leaderId);
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

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + id;
            return result;
        }

        public int GetRandomMemberId()
        {
            return Randomizer.Select(members.Keys);
        }

        public override bool Equals(object? obj)
        {
            return obj is Team other && other.getId() == id;
        }
    }
}
