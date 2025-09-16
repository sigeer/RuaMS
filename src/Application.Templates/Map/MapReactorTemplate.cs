namespace Application.Templates.Map
{
    public sealed class MapReactorTemplate
    {
        [WZPath("reactor/-/$name")]
        public int Index { get; set; }
        public int Id { get; set; }
        public string? Name { get; set; }
        public int ReactorTime { get; set; }
        public int F { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}