using Application.Utility.Exceptions;
using Quartz;
using Quartz.Impl;

namespace Application.Utility.Tasks;

public class QuartzTimerManager : ITimerManager
{
    IScheduler _scheduler;
    public QuartzTimerManager(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public async Task Start()
    {
        if (!_scheduler.IsStarted)
            await _scheduler.Start();
    }

    public async Task Stop()
    {
        if (!_scheduler.IsShutdown)
        {
            foreach (var scheduler in SchedulerManager.TaskScheduler)
            {
                await scheduler.Value.CancelAsync(false);
            }
            await _scheduler.Shutdown();
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
                        .WithIdentity(r!.Name);
        if (delay == null)
            builder.StartNow();
        else
            builder.StartAt(DateTimeOffset.Now.Add(delay.Value));

        var trigger = builder
                        .WithSimpleSchedule(x => x
                            .WithInterval(repeatTime) // 设置间隔为1秒
                            .RepeatForever()) // 一直重复执行
                        .Build();

        _scheduler.ScheduleJob(job, trigger).Wait();
        return SchedulerManager.TaskScheduler[r.Name] = new QuartzScheduledFuture(r!.Name);
    }

    public ScheduledFuture register(Action r, long repeatTime, long? delay = null) => register(TempRunnable.Parse(r), repeatTime, delay);
    public ScheduledFuture register(Action r, TimeSpan repeatTime, TimeSpan? delay = null) => register(TempRunnable.Parse(r), repeatTime, delay);

    public ScheduledFuture schedule(AbstractRunnable r, TimeSpan delay)
    {
        var job = JobBuilder.Create<QuartzJob>()
            .WithIdentity(r.Name)
            .UsingJobData(new JobDataMap() { { JobDataKeys.Data, r }, { JobDataKeys.IsRepeatable, false } })
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(r.Name)
            .StartAt(DateTimeOffset.Now.Add(delay))
            .Build();

        _scheduler.ScheduleJob(job, trigger).Wait();
        return SchedulerManager.TaskScheduler[r.Name] = new QuartzScheduledFuture(r!.Name);
    }

    public ScheduledFuture schedule(Action r, TimeSpan delay)
    {
        return schedule(TempRunnable.Parse(r), delay);
    }

    public ScheduledFuture schedule(Action r, long delay) => schedule(TempRunnable.Parse(r), TimeSpan.FromMilliseconds(delay));

    public ScheduledFuture scheduleAtTimestamp(AbstractRunnable r, DateTimeOffset time)
    {
        return schedule(r, (time - DateTimeOffset.Now));
    }

    public ScheduledFuture scheduleAtTimestamp(Action r, DateTimeOffset time)
    {
        return schedule(TempRunnable.Parse(r), time - DateTimeOffset.Now);
    }

    public bool isShutdown()
    {
        return _scheduler.IsShutdown;
    }
}
