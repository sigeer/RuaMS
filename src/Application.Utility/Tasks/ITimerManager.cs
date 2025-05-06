namespace Application.Utility.Tasks
{
    public interface ITimerManager
    {
        Task Start();
        Task Stop();
        ScheduledFuture register(AbstractRunnable r, long repeatDuration, long? delay = null);
        ScheduledFuture register(AbstractRunnable r, TimeSpan repeatDuration, TimeSpan? delay = null);

        ScheduledFuture register(Action r, long repeatDuration, long? delay = null);
        ScheduledFuture register(Action r, TimeSpan repeatDuration, TimeSpan? delay = null);


        ScheduledFuture schedule(AbstractRunnable r, TimeSpan delay);
        ScheduledFuture schedule(Action r, TimeSpan delay);
        ScheduledFuture schedule(Action r, long delay);
        ScheduledFuture scheduleAtTimestamp(AbstractRunnable r, DateTimeOffset time);
        ScheduledFuture scheduleAtTimestamp(Action r, DateTimeOffset time);

    }
}
