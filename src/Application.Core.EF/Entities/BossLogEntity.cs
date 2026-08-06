namespace Application.Core.EF.Entities
{
    public class BossLogEntity : IKeyedEntity<int>
    {
        private BossLogEntity()
        {
        }

        public BossLogEntity(int id, int characterId, string bosstype, int flag, DateTimeOffset time)
        {
            Id = id;
            CharacterId = characterId;
            BossType = bosstype;
            Flag = flag;
            Time = time;
        }
        public int Id { get; set; }

        public int CharacterId { get; set; }

        public string BossType { get; set; } = null!;
        public int Flag { get; set; }

        public DateTimeOffset Time { get; set; } = DateTimeOffset.UtcNow;
    }
}
