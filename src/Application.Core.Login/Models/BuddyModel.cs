namespace Application.Core.Login.Models
{
    public class BuddyModel
    {
        public int CharacterId { get; set; }
        public string CharacterName { get; set; }
        public sbyte Pending { get; set; }
        public string Group { get; set; }
    }
}
