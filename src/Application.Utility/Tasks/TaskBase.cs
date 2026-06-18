namespace Application.Utility.Tasks
{
    public abstract class TaskBase : IAsyncDisposable
    {
        protected ScheduledFuture? _scheduler;

        string _taskName;
        TimeSpan _repeatDuration;
        TimeSpan _repeatDelay;

        protected TaskBase(string taskName, TimeSpan repeatDuration, TimeSpan repeatDelay)
        {
            _repeatDuration = repeatDuration;
            _repeatDelay = repeatDelay;
            _taskName = taskName;
        }

        public virtual async ValueTask DisposeAsync()
        {
            await StopAsync();
        }

        public virtual async Task Register(ITimerManager timerManager)
        {
            _scheduler = await timerManager.RegisterAsync(new FuncAsyncRunnable(_taskName, HandleRun), _repeatDuration, _repeatDelay);
        }
        public virtual async Task StopAsync()
        {
            if (_scheduler != null)
                await _scheduler.CancelAsync(false);
        }

        protected abstract Task HandleRun();
    }
}
