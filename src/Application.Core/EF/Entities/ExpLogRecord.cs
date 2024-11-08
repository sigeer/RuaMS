namespace Application.Core.EF.Entities
{
    public class ExpLogRecord
    {
        private ExpLogRecord() { }
        public ExpLogRecord(int charId, int worldExpRate, int expCoupon, long gainedExp, int currentExp, DateTimeOffset expGainTime)
        {
            CharId = charId;
            WorldExpRate = worldExpRate;
            ExpCoupon = expCoupon;
            GainedExp = gainedExp;
            CurrentExp = currentExp;
            ExpGainTime = expGainTime;
        }
        public long Id { get; set; }

        public int CharId { get; set; }
        public int WorldExpRate { get; set; }
        public int ExpCoupon { get; set; }
        public long GainedExp { get; set; }

        public int CurrentExp { get; set; }
        public DateTimeOffset ExpGainTime { get; set; }

    }
}
