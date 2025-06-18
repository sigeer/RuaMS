namespace Application.Module.Family.Channel.Models
{
    public class PedigreeResult
    {
        public List<FamilyEntry> OrderedMembers { get; set; } = new();
        public List<(int ChrId, int TotalJuniors)> SuperJuniors { get; set; } = new();
        public int JuniorCount { get; set; }
        public int TotalSeniors { get; set; }
    }
}
