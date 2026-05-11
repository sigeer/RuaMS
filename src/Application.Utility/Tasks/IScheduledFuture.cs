namespace Application.Utility.Tasks
{
    public interface ScheduledFuture
    {
        string JobId { get; }
        Task<bool> CancelAsync(bool immediately);
        bool cancel(bool immediately);
    }
}
