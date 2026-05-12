using Application.Utility.Tasks;

namespace Application.Utility.Pipeline
{

    public interface IActorInstance<TContext> : INamedInstance where TContext: IActorInstance<TContext>
    {
        Task Send(ICommand command);
        Task Send(Func<TContext, Task> action);
        Task Send(Action<TContext> action);
    }
}
