namespace Application.Core.Game.Relation
{
    public class GuildMember
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Level { get; set; }
        public int JobId { get; set; }
        public int Channel { get; set; }

        public int GuildRank { get; set; }
        public int AllianceRank { get; set; }
        public int GP { get; set; }
    }
}
