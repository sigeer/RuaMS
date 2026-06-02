namespace Application.Core.scripting.Events.Abstraction
{
    public class TeamRegistry
    {
        public List<Player> EligibleMembers { get; }
        public TeamRegistry(List<Player> list)
        {
            EligibleMembers = list;
        }
    }
}
