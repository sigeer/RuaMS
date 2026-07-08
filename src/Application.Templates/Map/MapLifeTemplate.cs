namespace Application.Templates.Map
{
    public sealed class MapLifeTemplate
    {
        public int CY { get; set; }
        public bool Hide { get; set; }
        public int F { get; set; }
        [WZPath("~/fh")]
        public int Foothold { get; set; }

        public int MobTime { get; set; }
        public string Type { get; set; } = null!;

        [WZPath("~/rx0")]
        public int RX0 { get; set; }
        [WZPath("~/rx1")]
        public int RX1 { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Id { get; set; }
        public int Team { get; set; } = -1;
        [WZPath("~/$name")]
        public int Index { get; set; }
    }
}