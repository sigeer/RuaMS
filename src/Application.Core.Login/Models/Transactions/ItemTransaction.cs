namespace Application.Core.Login.Models.Transactions
{
    public class ItemTransaction
    {
        public long TransactionId { get; init; }
        public int PlayerId { get; init; }
        public List<ItemModel> CostItems { get; init; }
        public int CostMeso { get; init; }
        public DateTimeOffset CreatedAt { get; init; }

        public ItemTransaction(int playerId, List<ItemModel> items, int meso)
        {
            TransactionId = Yitter.IdGenerator.YitIdHelper.NextId();
            PlayerId = playerId;
            CreatedAt = DateTimeOffset.UtcNow;
            CostItems = items;
            CostMeso = meso;
        }
    }
}
