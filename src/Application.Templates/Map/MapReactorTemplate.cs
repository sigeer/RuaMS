namespace Application.Templates.Map
{
    public sealed class MapReactorTemplate
    {
        public int Index { get; set; }
        public int Id { get; set; }
        public string? Name { get; set; }
        public int ReactorTime { get; set; }
        public int F { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public MapReactorTemplate(int index)
        {
            Index = index;
        }
    }
}