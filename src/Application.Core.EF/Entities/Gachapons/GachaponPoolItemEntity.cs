namespace Application.Core.EF.Entities.Gachapons
{
    public class GachaponPoolItemEntity
    {
        private GachaponPoolItemEntity() { }
        public GachaponPoolItemEntity(int poolId, int level, int itemId, int quantity)
        {
            PoolId = poolId;
            Level = level;
            ItemId = itemId;
            Quantity = quantity;
        }

        public int Id { get; set; }
        public int PoolId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public int Level { get; set; }
    }
}
