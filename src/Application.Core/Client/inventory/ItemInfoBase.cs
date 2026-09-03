namespace client.inventory;

public readonly record struct InventoryItem(short Slot, Item Item);

public class TimedItemWrapper : IComparable<TimedItemWrapper>
{
    public Item Item { get; }
    public ItemTimedProperty Property { get; }
    public long TickTime
    {
        get
        {
            switch (Property)
            {
                case ItemTimedProperty.Always:
                    return 0;
                case ItemTimedProperty.Expiration:
                    return Item.getExpiration();
                case ItemTimedProperty.LockExpiration:
                    return Item.LockExpiration;
                default:
                    return 0;
            }
        }
    }

    public TimedItemWrapper(Item item, ItemTimedProperty property)
    {
        Item = item;
        Property = property;
    }

    public int CompareTo(TimedItemWrapper? other)
    {
        if (other == null) return 1;
        var r = TickTime.CompareTo(other.TickTime);
        if (r == 0)
            return Item.CompareTo(other.Item);
        return r;
    }
}

public enum ItemTimedProperty : byte
{
    Always = 0,
    Expiration = 1,
    LockExpiration
}