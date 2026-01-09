using Application.Core.Channel;
using Application.Core.Game.GameEvents.CPQ;

namespace Application.Core.Game.Relation
{
    public class Team
    {
        private int id;
        private int leaderId;
        private Dictionary<int, TeamMember> members = new();

        private Lock lockObj = new ();

        public Team(int id, int leaderId)
        {
            this.leaderId = leaderId;
            this.id = id;
        }

        public bool containsMembers(int memberId)
        {
            lockObj.Enter();
            try
            {
                return members.ContainsKey(memberId);
            }
            finally
            {
                lockObj.Exit();
            }
        }

        public void addMember(TeamMember member)
        {
            lockObj.Enter();
            try
            {
                members.TryAdd(member.Id, member);
            }
            finally
            {
                lockObj.Exit();
            }
        }

        public void removeMember(int member)
        {
            lockObj.Enter();
            try
            {
                members.Remove(member);
            }
            finally
            {
                lockObj.Exit();
            }
        }

        public void SetLeaderId(int leaderId)
        {
            this.leaderId = leaderId;
        }

        public void updateMember(TeamMember member)
        {
            lockObj.Enter();
            try
            {
                members[member.Id] = member;
            }
            finally
            {
                lockObj.Exit();
            }
        }

        public int GetMemberCount() => members.Count;
        public List<TeamMember> GetTeamMembers() => members.Values.ToList();

        /// <summary>
        /// 当需要获取完整Player时的获取队员，都会要求在同一频道
        /// </summary>
        /// <param name="currentServer"></param>
        /// <returns></returns>
        public List<Player> GetChannelMembers(WorldChannel currentServer)
        {
            lockObj.Enter();
            try
            {
                return members.Keys.Select(x => currentServer.Players.getCharacterById(x)).Where(x => x != null).ToList()!;
            }
            finally
            {
                lockObj.Exit();
            }
        }

        /// <summary>
        /// 在<see cref="server"/>上的队员
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public List<Player> GetActiveMembers(WorldChannelServer server)
        {
            lockObj.Enter();
            try
            {
                return members.Values.Select(x => server.FindPlayerById(x.Channel, x.Id)).Where(x => x != null).ToList()!;
            }
            finally
            {
                lockObj.Exit();
            }
        }


        public int getId()
        {
            return id;
        }

        public int getLeaderId()
        {
            return leaderId;
        }

        public TeamMember GetTeamMember(int id)
        {
            return members.GetValueOrDefault(id) ?? throw new BusinessException($"CharacterId = {id} 不在队伍里");
        }

        public Player? GetChannelLeader(WorldChannel server)
        {
            lockObj.Enter();
            try
            {
                return server.Players.getCharacterById(leaderId);
            }
            finally
            {
                lockObj.Exit();
            }
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + id;
            return result;
        }

        public override bool Equals(object? obj)
        {
            return obj is Team other && other.getId() == id;
        }
    }
}
