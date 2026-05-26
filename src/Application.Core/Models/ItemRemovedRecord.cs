using client.inventory;

namespace Application.Core.Models
{
    public record ItemRemovedRecord(int ItemId, short RemovedCount);
}
