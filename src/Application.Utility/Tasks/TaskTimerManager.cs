using Application.Utility.Exceptions;
using Serilog;
using System.Collections.Concurrent;

namespace Application.Utility.Tasks;

public class TaskTimerManager : ITimerManager
{
    public ConcurrentDictionary<string, ScheduledFuture> TaskScheduler { get; } = new();

    bool _isRunning = false;

    public async Task Start()
    {
        _isRunning = true;
        await Task.CompletedTask;
    }

    public async Task Stop()
    {
        _isRunning = false;
        foreach (var scheduler in TaskScheduler)
        {
            await scheduler.Value.CancelAsync(false);
        }
    }

    public void Purge(Action action)
    {
        action.Invoke();
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
        var ctx = new TimerTaskStatus();
        _ = Task.Run(async () =>
        {
            if (delay != null)
                await Task.Delay(delay.Value, ctx.LinkedCts.Token);

            while (!ctx.GracefulCts.IsCancellationRequested)
            {
                r.run();
                await Task.Delay(repeatTime, ctx.LinkedCts.Token);
            }
        }, ctx.ImmediateCts.Token)
                        .ContinueWith(t =>
                        {
                            if (TaskScheduler.TryRemove(r.Name, out var p))
                                Log.Logger.Debug("结束了一个任务【{TaskStatus}】，JobId = {JobId}", t.Status, p.JobId);
                        });
        return TaskScheduler[r.Name] = new TaskScheduledFuture(r!.Name, ctx);
    }

    public ScheduledFuture register(Action r, long repeatTime, long? delay = null) => register(TempRunnable.Parse(r), repeatTime, delay);
    public ScheduledFuture register(Action r, TimeSpan repeatTime, TimeSpan? delay = null) => register(TempRunnable.Parse(r), repeatTime, delay);

    public ScheduledFuture schedule(AbstractRunnable r, TimeSpan delay)
    {
        var ctx = new TimerTaskStatus();
        _ = Task.Run(async () =>
        {
            await Task.Delay(delay, ctx.LinkedCts.Token);
            r.run();
        }, ctx.ImmediateCts.Token)
            .ContinueWith(t =>
            {
                if (TaskScheduler.Remove(r.Name, out var p))
                    Log.Logger.Debug("结束了一个任务【{TaskStatus}】，JobId = {JobId}", t.Status, p.JobId);
            });
        return TaskScheduler[r.Name] = new TaskScheduledFuture(r!.Name, ctx);
    }

    public ScheduledFuture schedule(Action r, TimeSpan delay)
    {
        return schedule(TempRunnable.Parse(r), delay);
    }

    public ScheduledFuture schedule(Action r, long delay)
    {
        return schedule(TempRunnable.Parse(r), TimeSpan.FromMilliseconds(delay));
    }

    public ScheduledFuture scheduleAtTimestamp(AbstractRunnable r, DateTimeOffset time)
    {
        return schedule(r, (time - DateTimeOffset.UtcNow));
    }

    public ScheduledFuture scheduleAtTimestamp(Action r, DateTimeOffset time)
    {
        return schedule(TempRunnable.Parse(r), time - DateTimeOffset.UtcNow);
    }

}
