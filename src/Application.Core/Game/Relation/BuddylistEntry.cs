namespace Application.Core.Game.Relation;

public class BuddyCharacter
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Channel { get; set; }
    public int MapId { get; set; }
    public string Group { get; set; } = StringConstants.Buddy_DefaultGroup;
    public bool IsOnlined => Channel > 0;
}
