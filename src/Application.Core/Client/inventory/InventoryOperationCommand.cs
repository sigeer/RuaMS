using client.inventory;

namespace Application.Core.Client.inventory
{
    public interface IInventoryOperationCommand
    {
        byte Mode { get; }
        InventoryType InventoryType { get; }
        short CurrentPosition { get; }
    }

    public record InventoryAdd : IInventoryOperationCommand
    {
        public InventoryAdd(InventoryType inventoryType, Item item, short targetPosition)
        {
            InventoryType = inventoryType;
            Item = item.copy();
            CurrentPosition = targetPosition;
        }

        public byte Mode => 0;

        public InventoryType InventoryType { get; }
        public Item Item { get; }

        public short CurrentPosition { get; }
    }

    public record InventoryUpdateQuantity : IInventoryOperationCommand
    {
        public InventoryUpdateQuantity(InventoryType inventoryType, short targetPosition, short newQuantity)
        {
            InventoryType = inventoryType;
            CurrentPosition = targetPosition;
            NewQuantity = newQuantity;
        }

        public byte Mode => 1;

        public InventoryType InventoryType { get; }
        public short CurrentPosition { get; }

        public short NewQuantity { get; }
    }

    public record InventoryMove : IInventoryOperationCommand
    {
        public InventoryMove(InventoryType inventoryType, short targetPosition, short newPosition)
        {
            InventoryType = inventoryType;
            CurrentPosition = targetPosition;
            NewPosition = newPosition;
        }

        public byte Mode => 2;

        public InventoryType InventoryType { get; }

        public short CurrentPosition { get; }
        public short NewPosition { get; }
    }

    public record InventoryRemove : IInventoryOperationCommand
    {
        public InventoryRemove(InventoryType inventoryType, short targetPosition)
        {
            InventoryType = inventoryType;
            CurrentPosition = targetPosition;
        }

        public byte Mode => 3;

        public InventoryType InventoryType { get; }

        public short CurrentPosition { get; }
    }

    public record InventoryUpdateEquipExp : IInventoryOperationCommand
    {
        public InventoryUpdateEquipExp(short targetPosition, int newExp)
        {
            CurrentPosition = targetPosition;
            NewExp = newExp;
        }

        public byte Mode => 4;

        public InventoryType InventoryType => InventoryType.EQUIP;

        public short CurrentPosition { get; }
        public int NewExp { get; }
    }
}
