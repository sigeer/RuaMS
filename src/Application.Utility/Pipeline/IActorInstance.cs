namespace Application.Utility.Pipeline
{

    public interface IActorInstance<TContext> where TContext: IActorInstance<TContext>
    {
        string InstanceName { get; }
        Task Send(ICommand command);

        Task Send(Func<TContext, Task> action);
        Task Send(Action<TContext> action);
    }
}
