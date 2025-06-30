using Application.Core.Channel.Services;
using Application.Shared.Items;
using client.inventory;

namespace Application.Core.Channel.Transactions
{
    public class ItemTransaction
    {
        public long TransactionId { get; init; }
        public IPlayer Player { get; set; }
        public List<Item> CostItems { get; init; } = [];
        public int CostMeso { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public ItemTransactionStatus Status { get; set; }
    }
}
