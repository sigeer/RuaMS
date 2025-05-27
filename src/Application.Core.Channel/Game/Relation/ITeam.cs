using server.maps;

namespace Application.Core.Game.Relation
{
    /// <summary>
    /// 队伍
    /// </summary>
    public interface ITeam
    {
        void addDoor(int owner, Door door);
        void addMember(Player member);
        void assignNewLeader(ChannelClient c);
        bool containsMembers(Player member);
        bool Equals(object? obj);
        Dictionary<int, Door> getDoors();
        ICollection<Player> getEligibleMembers();

        int GetHashCode();
        int getId();
        Player getLeader();
        int getLeaderId();
        Player? getMemberById(int id);
        Player? getMemberByPos(int pos);
        ICollection<Player> getMembers();
        List<int> getMembersSortedByHistory();
        sbyte getPartyDoor(int cid);
        List<Player> getPartyMembersOnline();
        void removeDoor(int owner);
        void removeMember(Player member);
        void setEligibleMembers(List<Player> eliParty);
        void setId(int id);
        void setLeader(Player victim);
        void updateMember(Player member);
    }
}
