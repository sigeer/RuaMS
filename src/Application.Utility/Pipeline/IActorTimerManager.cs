using Application.Utility.Tasks;

namespace Application.Utility.Pipeline
{
    public interface IActorTimerManager<TContext> : INamedInstance where TContext : IActorInstance<TContext>
    {
        ScheduledFuture Schedule(string name, Action<TContext> r, TimeSpan delay);
        ScheduledFuture Schedule(string name, Func<TContext, Task> r, TimeSpan delay);
        ScheduledFuture Schedule(Action<TContext> r, TimeSpan delay);
        ScheduledFuture Schedule(Func<TContext, Task> r, TimeSpan delay);
        ScheduledFuture Register(string name, Action<TContext> r, TimeSpan period, TimeSpan delay);
        ScheduledFuture Register(string name, Func<TContext, Task> r, TimeSpan period, TimeSpan delay);
    }

}
