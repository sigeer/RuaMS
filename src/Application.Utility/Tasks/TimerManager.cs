using Application.Utility.Exceptions;
using DotNetty.Common.Utilities;
using Serilog;
using System.Collections.Concurrent;

namespace Application.Utility.Tasks
{
    public class TimerManager : ITimerManager
    {
        readonly HashedWheelTimer _timer;
        ConcurrentDictionary<JobKey, ScheduledFuture> _tasks;
        public TimerManager()
        {
            _timer = new HashedWheelTimer();
            _tasks = new();
        }

        public ScheduledFuture Schedule(string group, string name, Action r, TimeSpan delay)
        {
            var key = TimerUtils.GenerateKey(group, name);
            var p = new TimerTaskScheduledFuture(key, _timer.NewTimeout(new DelegatingTimerTask(() =>
            {
                try
                {
                    r();
                }
                finally
                {
                    _tasks.TryRemove(key, out _);
                }
            }), delay));
            if (_tasks.TryAdd(key, p))
            {
                return p;
            }
            throw new BusinessFatalException($"任务名重复: Group={group}, Name={name}");
        }
        public ScheduledFuture Register(string group, string name, Action r, TimeSpan period, TimeSpan delay)
        {
            var key = TimerUtils.GenerateKey(group, name);
            var p = new TimerScheduledFuture(key);
            if (_tasks.TryAdd(key, p))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(delay, p.Token);

                        using var timer = new PeriodicTimer(period);
                        while (await timer.WaitForNextTickAsync(p.Token))
                        {
                            r();
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex.ToString());
                    }
                    finally
                    {
                        _tasks.TryRemove(key, out _);
                    }
                }, p.Token);
                return p;
            }
            throw new BusinessFatalException($"任务名重复: Group={group}, Name={name}");
        }

        public ScheduledFuture Schedule(string group, string name, Func<Task> r, TimeSpan delay)
        {
            var key = TimerUtils.GenerateKey(group, name);
            var p = new TimerTaskScheduledFuture(key, _timer.NewTimeout(new DelegatingTimerTask(() =>
            {
                try
                {
                    r();
                }
                finally
                {
                    _tasks.TryRemove(key, out _);
                }
            }), delay));
            if (_tasks.TryAdd(key, p))
            {
                return p;
            }
            throw new BusinessFatalException($"任务名重复: Group={group}, Name={name}");
        }
        public ScheduledFuture Register(string group, string name, Func<Task> r, TimeSpan period, TimeSpan delay)
        {
            var key = TimerUtils.GenerateKey(group, name);
            var p = new TimerScheduledFuture(key);
            if (_tasks.TryAdd(key, p))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(delay, p.Token);

                        using var timer = new PeriodicTimer(period);
                        while (await timer.WaitForNextTickAsync(p.Token))
                        {
                            await r();
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex.ToString());
                    }
                    finally
                    {
                        _tasks.TryRemove(key, out _);
                    }
                }, p.Token);
                return p;
            }
            throw new BusinessFatalException($"任务名重复: Group={group}, Name={name}");
        }

        public void StopAll()
        {
            var tasks = _tasks.Values.ToList(); // 快照
            _tasks.Clear();
            foreach (var p in tasks)
            {
                p.cancel();
            }
        }
        public void StopGroup(string group)
        {
            var keys = _tasks.Keys.Where(x => x.Group == group).ToArray();
            foreach (var k in keys)
            {
                if (_tasks.TryRemove(k, out var d))
                {
                    d.cancel();
                }
            }
        }
        public List<string> GetAllJobs()
        {
            return _tasks.Keys.ToList().Select(x => $"{x.Group}.{x.Name}").ToList();
        }

        public async ValueTask DisposeAsync()
        {
            StopAll();
            await _timer.StopAsync();
        }
    }
}
