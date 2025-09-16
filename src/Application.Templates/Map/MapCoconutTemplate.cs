namespace Application.Templates.Map
{
    public sealed class MapCoconutTemplate
    {
        public string? EffectWin { get; set; }
        public string? EffectLose { get; set; }
        public string? SoundWin { get; set; }
        public string? SoundLose { get; set; }
        public int TimeDefault { get; set; }
        public int TimeExpand { get; set; }
        public int TimeFinish { get; set; }

        public int CountFalling { get; set; }
        public int CountBombing { get; set; }
        public int CountStopped { get; set; }
        public int CountHit { get; set; }
    }
}
