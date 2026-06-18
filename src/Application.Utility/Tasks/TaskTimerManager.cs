using Application.Utility.Exceptions;
using Serilog;
using System.Collections.Concurrent;

namespace Application.Utility.Tasks;

public class TaskTimerManager : ITimerManager
{
    public string Name => string.Empty;
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
    public async Task<ScheduledFuture> register(AbstractRunnable r, long repeatTime, long? delay = null) =>
        await register(r, TimeSpan.FromMilliseconds(repeatTime), delay == null ? null : TimeSpan.FromMilliseconds(delay.Value));

    public async Task<ScheduledFuture> register(AbstractRunnable r, TimeSpan repeatTime, TimeSpan? delay = null)
    {
        var ctx = new TimerTaskStatus();
        await Task.Run(async () =>
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

    public async Task<ScheduledFuture> register(Action r, long repeatTime, long? delay = null) => await register(TempRunnable.Parse(r), repeatTime, delay);
    public async Task<ScheduledFuture> register(Action r, TimeSpan repeatTime, TimeSpan? delay = null) => await register(TempRunnable.Parse(r), repeatTime, delay);

    public async Task<ScheduledFuture> RegisterAsync(AsyncAbstractRunnable r, long repeatTime, long? delay = null) =>
        await RegisterAsync(r, TimeSpan.FromMilliseconds(repeatTime), delay == null ? null : TimeSpan.FromMilliseconds(delay.Value));

    public async Task<ScheduledFuture> RegisterAsync(AsyncAbstractRunnable r, TimeSpan repeatTime, TimeSpan? delay = null)
    {
        var ctx = new TimerTaskStatus();
        await Task.Run(async () =>
        {
            if (delay != null)
                await Task.Delay(delay.Value, ctx.LinkedCts.Token);

            while (!ctx.GracefulCts.IsCancellationRequested)
            {
                await r.RunAsync();
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

    public async Task<ScheduledFuture> schedule(AbstractRunnable r, TimeSpan delay)
    {
        var ctx = new TimerTaskStatus();
        await Task.Run(async () =>
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

    public async Task<ScheduledFuture> schedule(Action r, TimeSpan delay)
    {
        return await schedule(TempRunnable.Parse(r), delay);
    }

    public async Task<ScheduledFuture> schedule(Action r, long delay)
    {
        return await schedule(TempRunnable.Parse(r), TimeSpan.FromMilliseconds(delay));
    }

    public async Task<ScheduledFuture> scheduleAtTimestamp(AbstractRunnable r, DateTimeOffset time)
    {
        return await schedule(r, (time - DateTimeOffset.UtcNow));
    }

    public async Task<ScheduledFuture> scheduleAtTimestamp(Action r, DateTimeOffset time)
    {
        return await schedule(TempRunnable.Parse(r), time - DateTimeOffset.UtcNow);
    }

    public async Task<ScheduledFuture> ScheduleAsync(string taskName, Func<Task> r, TimeSpan delay)
    {
        taskName = $"{taskName}_{r.GetHashCode()}";
        var ctx = new TimerTaskStatus();
        await Task.Run(async () =>
        {
            await Task.Delay(delay, ctx.LinkedCts.Token);
            await r();
        }, ctx.ImmediateCts.Token)
            .ContinueWith(t =>
            {
                if (TaskScheduler.Remove(taskName, out var p))
                    Log.Logger.Debug("结束了一个任务【{TaskStatus}】，JobId = {JobId}", t.Status, p.JobId);
            });
        var m = new TaskScheduledFuture(taskName, ctx); ;
        return TaskScheduler[taskName] = m;
    }
}
