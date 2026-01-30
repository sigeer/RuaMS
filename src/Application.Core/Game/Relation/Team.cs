using Application.Core.Channel;

namespace Application.Core.Game.Relation
{
    public class Team
    {
        private int id;
        private int leaderId;
        private Dictionary<int, TeamMember> members = new();

        public Team(int id, int leaderId)
        {
            this.leaderId = leaderId;
            this.id = id;
        }

        public bool containsMembers(int memberId)
        {
            return members.ContainsKey(memberId);
        }

        public void addMember(TeamMember member)
        {
            members.TryAdd(member.Id, member);
        }

        public void removeMember(int member)
        {
            members.Remove(member);
        }

        public void SetLeaderId(int leaderId)
        {
            this.leaderId = leaderId;
        }

        public void updateMember(TeamMember member)
        {
            members[member.Id] = member;
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
            return members.Keys.Select(x => currentServer.Players.getCharacterById(x)).Where(x => x != null).ToList()!;
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
            return server.Players.getCharacterById(leaderId);
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
