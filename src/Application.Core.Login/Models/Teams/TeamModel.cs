using Application.Shared.Team;

namespace Application.Core.Login.Models
{
    public class TeamModel
    {
        HashSet<int> _members;
        public TeamModel(int id, int leaderId)
        {
            Id = id;
            LeaderId = leaderId;
            _members = new HashSet<int> { leaderId };
        }

        public int Id { get; }

        public int LeaderId { get; set; }
        public bool CanJoin => _members.Count < 6;

        public bool TryAddMember(int playerId, out UpdateTeamCheckResult code)
        {
            code = UpdateTeamCheckResult.Success;
            if (!CanJoin)
                code = UpdateTeamCheckResult.Join_TeamMemberFull;

            if (_members.Add(playerId))
                return true;

            code = UpdateTeamCheckResult.Join_InnerError;
            return false;
        }

        public bool TryExpel(int leaderId, int memberId, out UpdateTeamCheckResult code)
        {
            code = UpdateTeamCheckResult.Success;
            if (leaderId != LeaderId)
                code = UpdateTeamCheckResult.Expel_NotLeader;

            if (_members.Remove(memberId))
                return true;

            code = UpdateTeamCheckResult.Expel_InnerError;
            return false;
        }

        public bool TryChangeLeader(int leaderId, int memberId, out UpdateTeamCheckResult code)
        {
            code = UpdateTeamCheckResult.Success;
            if (leaderId != LeaderId)
                code = UpdateTeamCheckResult.ChangeLeader_NotLeader;

            if (_members.Contains(memberId))
            {
                LeaderId = memberId;
                return true;
            }

            code = UpdateTeamCheckResult.ChangeLeader_MemberNotExsited;
            return false;
        }

        public int[] GetMembers()
        {
            return _members.ToArray();
        }

        public bool TryRemoveMember(int playerId, out UpdateTeamCheckResult code)
        {
            code = UpdateTeamCheckResult.Success;
            if (_members.Remove(playerId))
                return true;

            code = UpdateTeamCheckResult.Leave_InnerError;
            return false;
        }
    }
}
