namespace Application.Shared.MapObjects
{
    public interface ITimeLimitedEventMap
    {
        public int TimeDefault { get; set; }
        public int TimeExpand { get; set; }
        public int TimeFinish { get; set; }
    }
}
