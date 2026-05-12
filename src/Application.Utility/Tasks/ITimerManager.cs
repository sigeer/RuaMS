namespace Application.Utility.Tasks
{
    public interface ITimerManager: IAsyncDisposable
    {
        ScheduledFuture Register(string group, string name, Action r, TimeSpan period, TimeSpan delay);
        ScheduledFuture Register(string group, string name, Func<Task> r, TimeSpan period, TimeSpan delay);
        ScheduledFuture Schedule(string group, string name, Action r, TimeSpan delay);
        ScheduledFuture Schedule(string group, string name, Func<Task> r, TimeSpan delay);
        void StopAll();
        void StopGroup(string group);
        List<string> GetAllJobs();
    }
}