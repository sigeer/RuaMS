using Quartz;

namespace Application.Utility.Tasks
{
    public class QuartzScheduledFuture: ScheduledFuture
    {
        public QuartzTimerManager _timeManager;
        public TriggerKey TriggerKey { get; }
        public QuartzScheduledFuture(QuartzTimerManager timerManager, string jobId, TriggerKey triggerKey)
        {
            _timeManager = timerManager;
            JobId = jobId;
            TriggerKey = triggerKey;
        }

        public string JobId { get; set; }
        public async Task<bool> CancelAsync(bool immediately)
        {
            var jobKey = JobKey.Create(JobId);
            if (immediately)
                await _timeManager.Scheduler.Interrupt(jobKey);

            return await _timeManager.Scheduler.DeleteJob(jobKey);
        }

        public bool cancel(bool immediately)
        {
            return CancelAsync(immediately).Result;
        }
    }
}
