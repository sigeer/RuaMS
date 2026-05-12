using Application.Utility.Pipeline;

namespace Application.Utility.Tasks
{
    public abstract class TaskBase : IDisposable
    {
        protected ScheduledFuture? _scheduler;

        protected TimeSpan _repeatDuration;

        protected TaskBase(TimeSpan repeatDuration)
        {
            _repeatDuration = repeatDuration;
        }

        public virtual void Dispose()
        {
            _scheduler?.Dispose();
        }

        public abstract void Register(ITimerManager timerManager, TimeSpan repeatDelay);
        protected abstract void HandleRun();
    }

    public abstract class AsyncTaskBase : IDisposable
    {
        protected ScheduledFuture? _scheduler;

        protected TimeSpan _repeatDuration;

        protected AsyncTaskBase(TimeSpan repeatDuration)
        {
            _repeatDuration = repeatDuration;
        }

        public virtual void Dispose()
        {
            _scheduler?.Dispose();
        }

        public abstract void Register(ITimerManager timerManager, TimeSpan repeatDelay);

        protected abstract Task HandleRun();
    }

    public abstract class ActorTask<TActorContext> : TaskBase where TActorContext : IActorInstance<TActorContext>
    {
        protected readonly TActorContext _actor;
        readonly string _name;
        protected ActorTask(TActorContext actor, string name, TimeSpan repeatDuration) : base(repeatDuration)
        {
            _actor = actor;
            _name = name;
        }

        public override void Register(ITimerManager timerManager, TimeSpan repeatDelay)
        {
            _scheduler = timerManager.Register(_actor.InstanceName, _name, () => { _actor.Send(i => HandleRun()); }, _repeatDuration, repeatDelay);
        }
    }

    public abstract class ActorAsyncTask<TActorContext> : AsyncTaskBase where TActorContext : IActorInstance<TActorContext>
    {
        protected readonly TActorContext _actor;
        readonly string _name;
        protected ActorAsyncTask(TActorContext actor, string name, TimeSpan repeatDuration) : base(repeatDuration)
        {
            _actor = actor;
            _name = name;
        }

        public override void Register(ITimerManager timerManager, TimeSpan repeatDelay)
        {
            _scheduler = timerManager.Register(_actor.InstanceName, _name, async () => { await _actor.Send(async i => await HandleRun()); }, _repeatDuration, repeatDelay);
        }
    }
}
