namespace Application.Templates.Map
{
    public sealed class MapLifeTemplate
    {
        [WZPath("life/-/cy")]
        public int CY { get; set; }
        public bool Hide { get; set; }
        public int F { get; set; }
        [WZPath("life/-/fh")]
        public int Foothold { get; set; }

        public int MobTime { get; set; }
        public string Type { get; set; } = null!;

        [WZPath("life/-/rx0")]
        public int RX0 { get; set; }
        [WZPath("life/-/rx1")]
        public int RX1 { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Id { get; set; }
        public int Team { get; set; } = -1;
        [WZPath("life/-/$name")]
        public int Index { get; set; }
    }
}