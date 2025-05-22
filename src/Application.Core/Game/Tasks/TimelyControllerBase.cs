using server;

namespace Application.Core.Game.Tasks
{
    public abstract class TimelyControllerBase: IAsyncDisposable
    {
        protected ScheduledFuture? _scheduler;

        string _taskName;
        TimeSpan _repeatDuration;
        TimeSpan _repeatDelay;


        protected TimelyControllerBase(string taskName, TimeSpan repeatDuration, TimeSpan repeatDelay)
        {
            _repeatDuration = repeatDuration;
            _repeatDelay = repeatDelay;
            _taskName = taskName;
        }

        public virtual async ValueTask DisposeAsync()
        {
            await StopAsync();
        }

        public virtual void Register()
        {
            _scheduler = TimerManager.getInstance().register(new NamedRunnable(_taskName, HandleRun), _repeatDuration, _repeatDelay);
        }
        public virtual async Task StopAsync()
        {
            if (_scheduler != null)
                await _scheduler.CancelAsync(false);
        }

        protected abstract void HandleRun();
    }
}
