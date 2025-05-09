namespace Application.Shared.Relations
{
    public class TeamMember
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int JobId { get; set; }
        public int MapId { get; set; }
        public int Channel { get; set; }
        public bool IsOnlined { get; set; }
    }
}
