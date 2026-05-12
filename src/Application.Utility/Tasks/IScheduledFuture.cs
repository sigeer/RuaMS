namespace Application.Utility.Tasks
{
    public interface ScheduledFuture: IDisposable
    {
        JobKey JobId { get; }
        void cancel();
    }
}
