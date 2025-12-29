namespace Application.Core.Game.Relation;

public class BuddyList
{
    public IPlayer Owner { get; set; }
    public int Capacity { get; set; }
    /// <summary>
    /// 好友变动不太可能会并发，使用原始Dictionary
    /// </summary>
    private Dictionary<int, BuddyCharacter> buddies = new();
    public int Count => buddies.Count;

    public BuddyList(IPlayer owner)
    {
        Owner = owner;
        Capacity = Owner.BuddyCapacity;
    }

    public bool Contains(int characterId)
    {
        return buddies.ContainsKey(characterId);
    }

    public BuddyCharacter? Get(int characterId)
    {
        return buddies.GetValueOrDefault(characterId);
    }

    public BuddyCharacter? GetByName(string characterName)
    {
        return buddies.Values.FirstOrDefault(x => x.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
    }

    public void Set(BuddyCharacter entry)
    {
        buddies[entry.Id] = entry;
    }

    public void Remove(int characterId)
    {
        buddies.Remove(characterId);
    }

    public ICollection<BuddyCharacter> getBuddies()
    {
        return buddies.Values.ToList();
    }

    public bool isFull()
    {
        return buddies.Count >= Capacity;
    }

    public int[] getBuddyIds()
    {
        return buddies.Keys.ToArray();
    }

    public void LoadFromRemote(BuddyCharacter[] dbList)
    {
        buddies = new(dbList.ToDictionary(x => x.Id));
    }
}
