namespace Application.Core.Game.Relation;

public class BuddylistEntry
{
    public IPlayer Player { get; set; }
    public string Group { get; set; }
    public bool Visible { get; set; }

    public BuddylistEntry(IPlayer player, string group, bool visible)
    {
        Player = player;
        Group = group;
        Visible = visible;
    }

    public int getChannel() => Player.Channel;

    public bool isOnline() => Player.IsOnlined;
    public string getName() => Player.Name;

    public int getCharacterId() => Player.Id;

    public override int GetHashCode()
    {
        return 31 + Player.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is BuddylistEntry t && t.Player.Id == Player.Id;
    }
}
