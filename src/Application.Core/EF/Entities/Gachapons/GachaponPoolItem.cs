namespace Application.Core.EF.Entities.Gachapons
{
    public class GachaponPoolItem
    {
        private GachaponPoolItem() { }
        public GachaponPoolItem(int poolId, int level, int itemId)
        {
            PoolId = poolId;
            Level = level;
            ItemId = itemId;
        }

        public int Id { get; set; }
        public int PoolId { get; set; }
        public int ItemId { get; set; }
        public int Level { get; set; }
    }
}
