using Application.Core.Game.TheWorld;
using Application.Shared.Relations;
using server.maps;

namespace Application.Core.Game.Relation
{
    /// <summary>
    /// 队伍
    /// </summary>
    public interface ITeam
    {
        void addDoor(int owner, Door door);
        void addMember(TeamMember member);
        void AssignNewLeader();
        bool ContainsMember(int playerId);
        bool Equals(object? obj);
        Dictionary<int, Door> getDoors();


        int GetHashCode();
        int getId();
        int getLeaderId();
        TeamMember? getMemberById(int id);
        int GetRandomMemberId();
        ICollection<TeamMember> getMembers();
        List<int> getMembersSortedByHistory();
        sbyte getPartyDoor(int cid);
        void removeDoor(int owner);
        void removeMember(int member);

        ICollection<IPlayer> getEligibleMembers();
        void setEligibleMembers(List<IPlayer> eliParty);
        void updateMember(TeamMember member);

        /// <summary>
        /// 调用时确保所有人在同一频道
        /// </summary>
        /// <returns></returns>
        List<IPlayer> GetChannelMembers();
        IPlayer? GetLeader();
        IPlayer? GetChannelMember(int memberId);
        void BroadcastTeamMessage(string from, string message);
    }
}
