namespace Application.Utility.Tasks
{
    public class TimerTaskStatus
    {
        public TimerTaskStatus()
        {
            ImmediateCts = new CancellationTokenSource();
            GracefulCts = new CancellationTokenSource();
            LinkedCts = CancellationTokenSource.CreateLinkedTokenSource(ImmediateCts.Token, GracefulCts.Token);
        }

        public CancellationTokenSource LinkedCts { get; set; }
        public CancellationTokenSource ImmediateCts { get; set; }
        public CancellationTokenSource GracefulCts { get; set; }
    }
}
