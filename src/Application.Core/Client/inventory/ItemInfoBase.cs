namespace client.inventory;

public record ItemInfoBase(int Id, string Name);

public record InventoryItem(short Slot, Item Item);

public record InventorySlot(short Slot, Item? Item);