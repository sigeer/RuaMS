namespace Application.Utility.Tasks
{
    public abstract class TaskBase : IAsyncDisposable
    {
        protected ScheduledFuture? _scheduler;

        protected TimeSpan _repeatDuration;
        protected TimeSpan _repeatDelay;

        protected TaskBase(TimeSpan repeatDuration, TimeSpan repeatDelay)
        {
            _repeatDuration = repeatDuration;
            _repeatDelay = repeatDelay;
        }

        public virtual async ValueTask DisposeAsync()
        {
            await StopAsync();
        }

        public abstract void Register(TimerManager timerManager);
        public virtual async Task StopAsync()
        {
            if (_scheduler != null)
                await _scheduler.CancelAsync();
        }

        protected abstract void HandleRun();
    }

    public abstract class AsyncTaskBase : IAsyncDisposable
    {
        protected ScheduledFuture? _scheduler;

        protected TimeSpan _repeatDuration;
        protected TimeSpan _repeatDelay;

        protected AsyncTaskBase(TimeSpan repeatDuration, TimeSpan repeatDelay)
        {
            _repeatDuration = repeatDuration;
            _repeatDelay = repeatDelay;
        }

        public virtual async ValueTask DisposeAsync()
        {
            await StopAsync();
        }

        public abstract void Register(TimerManager timerManager);
        public virtual async Task StopAsync()
        {
            if (_scheduler != null)
                await _scheduler.CancelAsync();
        }

        protected abstract Task HandleRun();
    }
}
