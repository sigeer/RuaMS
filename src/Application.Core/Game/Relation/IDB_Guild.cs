namespace Application.Core.Game.Relation
{
    public interface IDB_Guild
    {
        public int GuildId { get; set; }

        public int Leader { get; set; }

        public int GP { get; set; }

        public int Logo { get; set; }

        public short LogoColor { get; set; }

        public string Name { get; set; }

        public string Rank1Title { get; set; }

        public string Rank2Title { get; set; }

        public string Rank3Title { get; set; }

        public string Rank4Title { get; set; }

        public string Rank5Title { get; set; }

        public int Capacity { get; set; }

        public int LogoBg { get; set; }

        public short LogoBgColor { get; set; }

        public string? Notice { get; set; }

        public long Signature { get; set; }

        public int AllianceId { get; set; }
    }
}
