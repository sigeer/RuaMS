using client.inventory;

namespace Application.Core.model
{
    public record GroupedItem(ItemInventoryType ItemInventoryType, List<int> GroupQuantity)
    {

    }
}
