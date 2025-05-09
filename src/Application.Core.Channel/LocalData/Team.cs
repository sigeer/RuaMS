using Application.Core.Game.Players;
using Application.Core.Game.Relation;
using Application.Core.Game.TheWorld;
using Application.Shared.Relations;
using Application.Utility;
using server.maps;

namespace Application.Core.Channel.LocalData
{
    public class Team : ITeam
    {
        private int id;
        private int leaderId;
        private Dictionary<int, TeamMember> members = new();
        private List<IPlayer> pqMembers = new();

        private Dictionary<int, int> histMembers = new();
        private int nextEntry = 0;

        private Dictionary<int, Door> doors = new();

        private object lockObj = new object();
        IWorldChannel _channelServer;

        public Team(IWorldChannel channel, int id, int leaderId)
        {
            _channelServer = channel;
            this.leaderId = leaderId;
            this.id = id;
        }

        public bool ContainsMember(int playerId)
        {
            Monitor.Enter(lockObj);
            try
            {
                return members.ContainsKey(playerId);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public void addMember(TeamMember player)
        {
            Monitor.Enter(lockObj);
            try
            {
                histMembers[player.Id] = nextEntry;
                nextEntry++;

                members[player.Id] = player;
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public void removeMember(int playerId)
        {
            Monitor.Enter(lockObj);
            try
            {
                histMembers.Remove(playerId);

                members.Remove(playerId);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        internal void SetLeader(int victim)
        {
            this.leaderId = victim;
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

        public TeamMember? getMemberById(int id)
        {
            Monitor.Enter(lockObj);
            try
            {
                return members.GetValueOrDefault(id);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public ICollection<TeamMember> getMembers()
        {
            Monitor.Enter(lockObj);
            try
            {
                return members.Values.ToList();
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

        public int getLeaderId()
        {
            return leaderId;
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
                this.doors[owner] = door;
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

        public void AssignNewLeader()
        {
            Monitor.Enter(lockObj);
            try
            {
                var newPlayer = members.Values.Where(x => x.Id != leaderId).OrderByDescending(x => x.Level).FirstOrDefault();

                if (newPlayer != null)
                {
                    _channelServer.UpdateTeamGlobalData(this.getId(), PartyOperation.CHANGE_LEADER, newPlayer.Id, newPlayer.Name);
                }
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


        public List<IPlayer> GetChannelMembers()
        {
            return members.Keys.Select(x => _channelServer.Players.getCharacterById(x)).Where(x => x != null).ToList();
        }

        public IPlayer? GetLeader()
        {
            return GetChannelMember(leaderId);
        }

        public IPlayer? GetChannelMember(int memberId)
        {
            return members.ContainsKey(memberId) ? _channelServer.Players.getCharacterById(memberId) : null;
        }
    }
}
