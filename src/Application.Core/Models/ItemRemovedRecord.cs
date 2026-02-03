using client.inventory;

namespace Application.Core.Models
{
    public record ItemRemovedRecord(Item Item, bool AllowZero, short RemovedCount);
}
