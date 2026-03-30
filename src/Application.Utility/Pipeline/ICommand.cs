namespace Application.Utility.Pipeline
{
    public interface ICommand
    {
    }
    public interface ICommand<TContext>: ICommand where TContext : IActorInstance<TContext>
    {
        void Execute(TContext ctx);
    }

    public interface IAsyncCommand<TContext> : ICommand where TContext: IActorInstance<TContext>
    {
        Task Execute(TContext ctx);
    }
}
