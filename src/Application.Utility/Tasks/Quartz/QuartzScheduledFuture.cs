using Quartz;

namespace Application.Utility.Tasks
{
    public class QuartzScheduledFuture: ScheduledFuture
    {
        public QuartzScheduledFuture(string jobId)
        {
            JobId = jobId;
        }

        public string JobId { get; set; }
        public async Task<bool> CancelAsync(bool immediately)
        {
            var jobKey = JobKey.Create(JobId);
            if (immediately)
                await QuartzSchedulerManager.Scheduler.Interrupt(jobKey);

            return await QuartzSchedulerManager.Scheduler.DeleteJob(jobKey);
        }

        public bool cancel(bool immediately)
        {
            return CancelAsync(immediately).Result;
        }
    }
}
