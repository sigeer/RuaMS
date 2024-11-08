namespace Application.Core.EF.Entities.Gachapons
{
    public class GachaponPoolLevelChance
    {
        public int Id { get; set; }
        public int PoolId { get; set; }
        public int Level { get; set; }
        public int Chance { get; set; }
    }
}
