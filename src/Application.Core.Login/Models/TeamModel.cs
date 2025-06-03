namespace Application.Core.Login.Models
{
    public class TeamModel
    {
        public int Id { get; set; }
        public List<int> Members { get; set; } = [];
        public int LeaderId { get; set; }
    }
}
