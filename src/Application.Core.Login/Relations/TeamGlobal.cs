using Application.Core.Game.Relation;

namespace Application.Core.Login.Relations
{
    /// <summary>
    /// 所有的队伍，相比之前的Team，由于不能直接获取到Player，内部只存id
    /// </summary>
    internal class TeamGlobal : ITeamGlobal
    {
        HashSet<int> _members;
        public TeamGlobal(int id, int leaderId)
        {
            Id = id;
            LeaderId = leaderId;
            _members = new HashSet<int> { leaderId };
        }

        public int Id { get; }

        public int LeaderId { get; set; }

        public bool AddMember(int playerId)
        {
            return _members.Add(playerId);
        }

        public bool ChangeLeader(int memberId)
        {
            if (_members.Contains(memberId))
            {
                LeaderId = memberId;
                return true;
            }
            return false;
        }

        public ICollection<int> GetMembers()
        {
            throw new NotImplementedException();
        }

        public bool RemoveMember(int playerId)
        {
            return _members.Remove(playerId);
        }
    }
}
