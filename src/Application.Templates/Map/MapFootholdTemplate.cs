namespace Application.Templates.Map
{
    public sealed class MapFootholdTemplate
    {
        [WZPath("foothold/-/$name")]
        public int Index { get; set; }
        public int Next { get; set; }
        public int Prev { get; set; }
        public int X1 { get; set; }
        public int X2 { get; set; }
        public int Y1 { get; set; }
        public int Y2 { get; set; }
    }
}