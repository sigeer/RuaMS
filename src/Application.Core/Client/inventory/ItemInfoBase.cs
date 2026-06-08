namespace client.inventory;

public readonly record struct InventoryItem(short Slot, Item Item);

public class TimedItemWrapper : IComparable<TimedItemWrapper>
{
    public Item Item { get; }
    public long TickTime { get; }

    public TimedItemWrapper(Item item, long tickTime)
    {
        Item = item;
        TickTime = tickTime;
    }

    public int CompareTo(TimedItemWrapper? other)
    {
        if (other == null) return 1;
        return TickTime.CompareTo(other.TickTime);
    }
}