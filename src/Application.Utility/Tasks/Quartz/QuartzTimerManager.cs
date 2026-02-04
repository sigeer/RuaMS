using Quartz;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Application.Utility.Tasks;

public class QuartzTimerManager : ITimerManager
{
    public IScheduler Scheduler { get; }

    public ConcurrentDictionary<string, ScheduledFuture> TaskScheduler { get; } = new();
    public QuartzTimerManager(IScheduler scheduler)
    {
        Scheduler = scheduler;
    }

    public async Task Start()
    {
        if (!Scheduler.IsStarted)
            await Scheduler.Start();
    }

    public async Task Stop()
    {
        if (!Scheduler.IsShutdown)
        {
            foreach (var scheduler in TaskScheduler)
            {
                await scheduler.Value.CancelAsync(false);
            }
            await Scheduler.Shutdown();
        }
    }

    /// <summary>
    /// delay毫秒之后执行，并且每隔repeatTime毫秒之后执行
    /// </summary>
    /// <param name="r"></param>
    /// <param name="repeatTime"></param>
    /// <param name="delay"></param>
    /// <returns>job id</returns>
    public ScheduledFuture register(AbstractRunnable r, long repeatTime, long? delay = null) =>
        register(r, TimeSpan.FromMilliseconds(repeatTime), delay == null ? null : TimeSpan.FromMilliseconds(delay.Value));

    public ScheduledFuture register(AbstractRunnable r, TimeSpan repeatTime, TimeSpan? delay = null)
    {
        var job = JobBuilder.Create<QuartzJob>()
            .WithIdentity(r.Name)
            .UsingJobData(new JobDataMap() { { JobDataKeys.Data, r }, { JobDataKeys.IsRepeatable, true } })
            .Build();

        var builder = TriggerBuilder.Create()
                        .WithIdentity("T_" + r.Name);
        if (delay == null)
            builder.StartNow();
        else
            builder.StartAt(DateTimeOffset.UtcNow.Add(delay.Value));

        var trigger = builder
                        .WithSimpleSchedule(x => x
                            .WithInterval(repeatTime) // 设置间隔为1秒
                            .RepeatForever()) // 一直重复执行
                        .Build();

        Scheduler.ScheduleJob(job, trigger).ConfigureAwait(false).GetAwaiter().GetResult();
        return TaskScheduler[r.Name] = new QuartzScheduledFuture(this, r!.Name, trigger.Key);
    }

    public ScheduledFuture register(Action r, long repeatTime, long? delay = null) => register(TempRunnable.Parse(r), repeatTime, delay);
    public ScheduledFuture register(Action r, TimeSpan repeatTime, TimeSpan? delay = null) => register(TempRunnable.Parse(r), repeatTime, delay);

    public async Task<ScheduledFuture> RegisterAsync(AsyncAbstractRunnable r, long repeatTime, long? delay = null) =>
        await RegisterAsync(r, TimeSpan.FromMilliseconds(repeatTime), delay == null ? null : TimeSpan.FromMilliseconds(delay.Value));
    public async Task<ScheduledFuture> RegisterAsync(AsyncAbstractRunnable r, TimeSpan repeatTime, TimeSpan? delay = null)
    {
        var job = JobBuilder.Create<QuartzJob>()
            .WithIdentity(r.Name)
            .UsingJobData(new JobDataMap() { { JobDataKeys.Data, r }, { JobDataKeys.IsRepeatable, true } })
            .Build();

        var builder = TriggerBuilder.Create()
                        .WithIdentity("T_" + r.Name);
        if (delay == null)
            builder.StartNow();
        else
            builder.StartAt(DateTimeOffset.UtcNow.Add(delay.Value));

        var trigger = builder
                        .WithSimpleSchedule(x => x
                            .WithInterval(repeatTime) // 设置间隔为1秒
                            .RepeatForever()) // 一直重复执行
                        .Build();

        await Scheduler.ScheduleJob(job, trigger);
        return TaskScheduler[r.Name] = new QuartzScheduledFuture(this, r!.Name, trigger.Key);
    }


    public ScheduledFuture schedule(AbstractRunnable r, TimeSpan delay)
    {
        var job = JobBuilder.Create<QuartzJob>()
            .WithIdentity(r.Name)
            .UsingJobData(new JobDataMap() { { JobDataKeys.Data, r }, { JobDataKeys.IsRepeatable, false } })
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity("T_" + r.Name)
            .StartAt(DateTimeOffset.UtcNow.Add(delay))
            .Build();

        Scheduler.ScheduleJob(job, trigger).ConfigureAwait(false).GetAwaiter().GetResult();
        return TaskScheduler[r.Name] = new QuartzScheduledFuture(this, r!.Name, trigger.Key);
    }

    public ScheduledFuture schedule(Action r, TimeSpan delay)
    {
        return schedule(TempRunnable.Parse(r), delay);
    }

    public async Task<ScheduledFuture> ScheduleAsync(string taskName, Func<Task> r, TimeSpan delay)
    {
        taskName = $"{taskName}_{r.GetHashCode()}";
        var job = JobBuilder.Create<QuartzJob>()
            .WithIdentity(taskName)
            .UsingJobData(new JobDataMap() { { JobDataKeys.Data, r }, { JobDataKeys.IsRepeatable, false } })
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity("T_" + taskName)
            .StartAt(DateTimeOffset.UtcNow.Add(delay))
            .Build();

        await Scheduler.ScheduleJob(job, trigger);
        return TaskScheduler[taskName] = new QuartzScheduledFuture(this, taskName, trigger.Key);
    }

    public ScheduledFuture schedule(Action r, long delay) => schedule(TempRunnable.Parse(r), TimeSpan.FromMilliseconds(delay));

    public ScheduledFuture scheduleAtTimestamp(AbstractRunnable r, DateTimeOffset time)
    {
        return schedule(r, (time - DateTimeOffset.UtcNow));
    }

    public ScheduledFuture scheduleAtTimestamp(Action r, DateTimeOffset time)
    {
        return schedule(TempRunnable.Parse(r), time - DateTimeOffset.UtcNow);
    }

    public bool isShutdown()
    {
        return Scheduler.IsShutdown;
    }
}
