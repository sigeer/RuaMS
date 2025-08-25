using System.Collections.Concurrent;

namespace Application.Utility.Tasks
{
    public interface ITimerManager
    {
        ConcurrentDictionary<string, ScheduledFuture> TaskScheduler { get; }
        Task Start();
        Task Stop();
        ScheduledFuture register(AbstractRunnable r, long repeatTime, long? delay = null);
        ScheduledFuture register(AbstractRunnable r, TimeSpan repeatTime, TimeSpan? delay = null);

        ScheduledFuture register(Action r, long repeatTime, long? delay = null);
        ScheduledFuture register(Action r, TimeSpan repeatTime, TimeSpan? delay = null);


        ScheduledFuture schedule(AbstractRunnable r, TimeSpan delay);
        ScheduledFuture schedule(Action r, TimeSpan delay);
        ScheduledFuture schedule(Action r, long delay);
        ScheduledFuture scheduleAtTimestamp(AbstractRunnable r, DateTimeOffset time);
        ScheduledFuture scheduleAtTimestamp(Action r, DateTimeOffset time);

    }
}
