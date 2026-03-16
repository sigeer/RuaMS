using Application.Utility.Tasks;

namespace Application.Utility.Pipeline
{
    public interface IActor<TContext> where TContext: ICommandContext
    {
        ITimerManager TimerManager { get; }
        void Post(ICommand<TContext> command);
    }
}
