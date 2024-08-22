using Quartz;

namespace Application.Core.Compatible
{
    public class SchedulerManage
    {
        public static IScheduler Scheduler { get; set; } = null!;
    }
}
