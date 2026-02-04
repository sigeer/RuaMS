namespace Application.Utility.Pipeline
{
    public interface IActor<TContext> where TContext: ICommandContext
    {
        void Post(ICommand<TContext> command);
    }
}
