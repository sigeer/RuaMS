namespace Application.Core.EF.Entities.Gachapons
{
    public class GachaponPoolItem
    {
        private GachaponPoolItem() { }
        public GachaponPoolItem(int poolId, int itemId)
        {
            PoolId = poolId;
            ItemId = itemId;
        }

        public int Id { get; set; }
        public int PoolId { get; set; }
        public int ItemId { get; set; }
    }
}
