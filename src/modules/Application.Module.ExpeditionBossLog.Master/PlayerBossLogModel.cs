namespace Application.Module.ExpeditionBossLog.Master
{
    public class PlayerBossLogModel
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public string BossName { get; set; }
        public int Flag { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}
