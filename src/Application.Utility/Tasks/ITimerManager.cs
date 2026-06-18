using System.Collections.Concurrent;

namespace Application.Utility.Tasks
{
    public interface ITimerManager
    {
        string Name { get; }
        ConcurrentDictionary<string, ScheduledFuture> TaskScheduler { get; }
        Task Start();
        Task Stop();
        Task<ScheduledFuture> register(AbstractRunnable r, long repeatTime, long? delay = null);
        Task<ScheduledFuture> register(AbstractRunnable r, TimeSpan repeatTime, TimeSpan? delay = null);

        Task<ScheduledFuture> register(Action r, long repeatTime, long? delay = null);
        Task<ScheduledFuture> register(Action r, TimeSpan repeatTime, TimeSpan? delay = null);

        Task<ScheduledFuture> RegisterAsync(AsyncAbstractRunnable r, long repeatTime, long? delay = null);
        Task<ScheduledFuture> RegisterAsync(AsyncAbstractRunnable r, TimeSpan repeatTime, TimeSpan? delay = null);
        Task<ScheduledFuture> schedule(AbstractRunnable r, TimeSpan delay);
        Task<ScheduledFuture> schedule(Action r, TimeSpan delay);
        Task<ScheduledFuture> ScheduleAsync(string taskName, Func<Task> r, TimeSpan delay);
        Task<ScheduledFuture> schedule(Action r, long delay);
        Task<ScheduledFuture> scheduleAtTimestamp(AbstractRunnable r, DateTimeOffset time);
        Task<ScheduledFuture> scheduleAtTimestamp(Action r, DateTimeOffset time);

    }
}
