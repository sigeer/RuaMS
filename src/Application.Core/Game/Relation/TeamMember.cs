namespace Application.Core.Game.Relation
{
    public class TeamMember
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Level { get; set; }
        public int JobId { get; set; }
        public int Channel { get; set; }
    }
}
