namespace Application.Utility.Pipeline
{

    public interface IActorInstance<TContext> where TContext: IActorInstance<TContext>
    {
        string InstanceName { get; }
        void Send(ICommand command);

        void Send(Func<TContext, Task> action);
        void Send(Action<TContext> action);
    }
}
