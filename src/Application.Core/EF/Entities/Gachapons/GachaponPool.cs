namespace Application.Core.EF.Entities.Gachapons
{
    public class GachaponPool
    {
        public int Id { get; set; }
        /// <summary>
        /// -1：全局
        /// </summary>
        public int NpcId { get; set; }
        public int Level { get; set; }
        public int LevelChance { get; set; }
        public string? Comment { get; set; }
    }
}
