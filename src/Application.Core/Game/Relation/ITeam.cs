using server.maps;

namespace Application.Core.Game.Relation
{
    /// <summary>
    /// 队伍
    /// </summary>
    public interface ITeam
    {
        void addDoor(int owner, Door door);
        void addMember(IPlayer member);
        void assignNewLeader(IChannelClient c);
        bool containsMembers(IPlayer member);
        bool Equals(object? obj);
        Dictionary<int, Door> getDoors();
        ICollection<IPlayer> getEligibleMembers();

        int GetHashCode();
        int getId();
        IPlayer getLeader();
        int getLeaderId();
        IPlayer? getMemberById(int id);
        IPlayer? getMemberByPos(int pos);
        ICollection<IPlayer> getMembers();
        List<int> getMembersSortedByHistory();
        sbyte getPartyDoor(int cid);
        List<IPlayer> getPartyMembersOnline();
        void removeDoor(int owner);
        void removeMember(IPlayer member);
        void setEligibleMembers(List<IPlayer> eliParty);
        void setId(int id);
        void setLeader(IPlayer victim);
        void updateMember(IPlayer member);
    }
}
