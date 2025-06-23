using Application.Utility;

namespace Application.Module.ExpeditionBossLog.Master
{
    public class BossLogEntry : EnumClass
    {
        public static readonly BossLogEntry ZAKUM = new BossLogEntry(2, 1, false);
        public static readonly BossLogEntry HORNTAIL = new BossLogEntry(2, 1, false);
        public static readonly BossLogEntry PINKBEAN = new BossLogEntry(1, 1, false);
        public static readonly BossLogEntry SCARGA = new BossLogEntry(1, 1, false);
        public static readonly BossLogEntry PAPULATUS = new BossLogEntry(2, 1, false);

        private int entries;
        private int timeLength;
        private int minChannel;
        private int maxChannel;
        private bool week;

        public int MinChannel { get => minChannel; set => minChannel = value; }
        public int MaxChannel { get => maxChannel; set => maxChannel = value; }
        public int Entries { get => entries; set => entries = value; }
        public bool Week { get => week; set => week = value; }

        BossLogEntry(int entries, int timeLength, bool week) : this(entries, 0, int.MaxValue, timeLength, week)
        {

        }

        BossLogEntry(int entries, int minChannel, int maxChannel, int timeLength, bool week)
        {
            this.entries = entries;
            this.minChannel = minChannel;
            this.maxChannel = maxChannel;
            this.timeLength = timeLength;
            this.week = week;
        }
    }

}
