using Application.Shared.Constants;

namespace Application.Core.Login.Models
{
    public class BuddyModel
    {
        public int Id { get; set; }
        public string Group { get; set; } = StringConstants.Buddy_DefaultGroup;
        public int CharacterId { get; set; }
    }
}
