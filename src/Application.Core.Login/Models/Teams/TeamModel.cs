using Application.Shared.Constants;
using Application.Shared.Team;

namespace Application.Core.Login.Models
{
    public class TeamModel
    {
        HashSet<CharacterLiveObject> _members;
        public TeamModel(int id, CharacterLiveObject leaderId)
        {
            Id = id;
            LeaderId = leaderId.Character.Id;
            _members = new HashSet<CharacterLiveObject> { leaderId };
        }

        public int Id { get; }

        public int LeaderId { get; set; }
        public bool CanJoin => _members.Count < Limits.MaxTeamMember;

        public bool TryAddMember(CharacterLiveObject player, out UpdateTeamCheckResult code)
        {
            code = UpdateTeamCheckResult.Success;
            if (player.Character.Party > 0)
            {
                code = UpdateTeamCheckResult.Join_HasTeam;
                return false;
            }

            if (!CanJoin)
            {
                code = UpdateTeamCheckResult.Join_TeamMemberFull;
                return false;
            }

            if (_members.Add(player))
            {
                player.Character.Party = Id;
                return true;
            }

            code = UpdateTeamCheckResult.Join_InnerError;
            return false;
        }

        public bool TryExpel(int leaderId, CharacterLiveObject member, out UpdateTeamCheckResult code)
        {
            code = UpdateTeamCheckResult.Success;
            if (leaderId != LeaderId)
            {
                code = UpdateTeamCheckResult.Expel_NotLeader;
                return false;
            }

            if (_members.Remove(member))
            {
                member.Character.Party = 0;
                return true;
            }

            code = UpdateTeamCheckResult.Expel_InnerError;
            return false;
        }

        public bool TryChangeLeader(int leaderId, CharacterLiveObject memberId, out UpdateTeamCheckResult code)
        {
            code = UpdateTeamCheckResult.Success;
            if (leaderId != LeaderId)
            {
                code = UpdateTeamCheckResult.ChangeLeader_NotLeader;
                return false;
            }

            if (_members.Contains(memberId))
            {
                LeaderId = memberId.Character.Id;
                return true;
            }

            code = UpdateTeamCheckResult.ChangeLeader_MemberNotExsited;
            return false;
        }

        public int[] GetMembers()
        {
            return _members.Select(x => x.Character.Id).ToArray();
        }

        public IEnumerable<CharacterLiveObject> GetMemberObjectss()
        {
            return _members;
        }

        public bool TryRemoveMember(CharacterLiveObject member, out UpdateTeamCheckResult code)
        {
            code = UpdateTeamCheckResult.Success;
            if (_members.Remove(member))
            {
                member.Character.Party = 0;
                return true;
            }

            code = UpdateTeamCheckResult.Leave_InnerError;
            return false;
        }

        public void Disband()
        {
            foreach (var item in _members)
            {
                item.Character.Party = 0;
            }
        }
    }
}
