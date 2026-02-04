namespace Application.Utility.Pipeline
{
    public interface ICommand<TContext> where TContext: ICommandContext
    {
        void Execute(TContext ctx);
    }
}
