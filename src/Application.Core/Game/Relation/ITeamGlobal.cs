namespace Application.Core.Game.Relation
{
    public interface ITeamGlobal
    {
        int Id { get; }
        int LeaderId { get; set; }
        ICollection<int> GetMembers();
        bool AddMember(int playerId);
        bool RemoveMember(int playerId);
        bool ChangeLeader(int memberId);
    }
}
