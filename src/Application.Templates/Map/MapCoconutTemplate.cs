namespace Application.Templates.Map
{
    public sealed class MapCoconutTemplate : MapEffectTemplateBase
    {
        public int TimeDefault { get; set; }
        public int TimeExpand { get; set; }
        public int TimeFinish { get; set; }

        public int CountFalling { get; set; }
        public int CountBombing { get; set; }
        public int CountStopped { get; set; }
        public int CountHit { get; set; }
    }
}
