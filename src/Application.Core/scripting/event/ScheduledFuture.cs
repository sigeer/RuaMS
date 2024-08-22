using Quartz;

namespace Application.Core.scripting.Event
{
    public class ScheduledFuture
    {
        public ScheduledFuture(string jobId)
        {
            JobId = jobId;
        }

        public string JobId { get; set; }
        public async Task<bool> cancelAsync(bool immediately)
        {
            var jobKey = JobKey.Create(JobId);
            if (immediately)
                await SchedulerManage.Scheduler.Interrupt(jobKey);

            return await SchedulerManage.Scheduler.DeleteJob(jobKey);
        }

        public bool cancel(bool immediately)
        {
            return cancelAsync(immediately).Result;
        }
    }
}
