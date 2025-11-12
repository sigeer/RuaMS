namespace Application.Core.Models
{
    public class GachaponDataObject
    {
        public int Id { get; set; }
        /// <summary>
        /// -1：全局
        /// </summary>
        public int NpcId { get; set; }
        public string Name { get; set; } = null!;
    }

    public class GachaponPoolLevelChanceDataObject
    {
        public int PoolId { get; set; }
        public int Level { get; set; }
        public int Chance { get; set; }
    }

    public class GachaponPoolItemDataObject
    {
        public int PoolId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public int Level { get; set; }
    }
}
