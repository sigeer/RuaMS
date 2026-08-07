using Application.Shared.Events;
using Application.Utility;

namespace Application.Shared.Battle
{
    public class ExpeditionEntryType : EnumClass
    {
        public static readonly ExpeditionEntryType BALROG_EASY = new(nameof(BALROG_EASY), 1, false);
        public static readonly ExpeditionEntryType BALROG_NORMAL = new(nameof(BALROG_NORMAL), 1, false);

        public static readonly ExpeditionEntryType ZAKUM = new (nameof(ZAKUM), 2, false);
        public static readonly ExpeditionEntryType HORNTAIL = new (nameof(HORNTAIL), 2, false);
        public static readonly ExpeditionEntryType PINKBEAN = new (nameof(PINKBEAN), 1, false);
        public static readonly ExpeditionEntryType SCARGA = new (nameof(SCARGA), 1, false);
        public static readonly ExpeditionEntryType PAPULATUS = new (nameof(PAPULATUS), 2, false);

        public string Name { get; }
        private int entries;
        private int minChannel;
        private int maxChannel;
        private bool week;

        public int MinChannel { get => minChannel; set => minChannel = value; }
        public int MaxChannel { get => maxChannel; set => maxChannel = value; }
        public int Entries { get => entries; set => entries = value; }
        public bool Week { get => week; set => week = value; }

        ExpeditionEntryType(string name, int entries, bool week) : this(name, entries, 0, int.MaxValue,  week)
        {

        }

        ExpeditionEntryType(string name, int entries, int minChannel, int maxChannel, bool week)
        {
            Name = name;
            this.entries = entries;
            this.minChannel = minChannel;
            this.maxChannel = maxChannel;
            this.week = week;
        }
    }

}
