using System.Collections.Concurrent;

namespace Application.Utility.Tasks
{
    public class SchedulerManager
    {
        public static Dictionary<string, ScheduledFuture> TaskScheduler { get; set; } = new();
    }
}
