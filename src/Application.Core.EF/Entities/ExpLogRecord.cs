namespace Application.Core.EF.Entities
{
    public class ExpLogRecord
    {
        private ExpLogRecord() { }
        public ExpLogRecord(int charId, float worldExpRate, float expCoupon, long gainedExp, int currentExp, DateTimeOffset expGainTime)
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
        public float WorldExpRate { get; set; }
        public float ExpCoupon { get; set; }
        public long GainedExp { get; set; }

        public int CurrentExp { get; set; }
        public DateTimeOffset ExpGainTime { get; set; }

    }
}
