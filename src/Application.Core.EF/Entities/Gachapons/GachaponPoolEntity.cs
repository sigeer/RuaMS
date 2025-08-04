namespace Application.Core.EF.Entities.Gachapons
{
    public class GachaponPoolEntity
    {
        public int Id { get; set; }
        /// <summary>
        /// -1：全局
        /// </summary>
        public int NpcId { get; set; }
        public string Name { get; set; } = null!;
    }
}
