using Application.Core.constants;
using Quartz;

namespace Application.Core.scripting.Event.jobs
{
    public class JobCompleteListener : IJobListener
    {
        public string Name => "JobCompleteListener";

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
        {
            var scheduler = context.Scheduler;
            if (context.JobDetail.JobDataMap[JobDataKeys.IsRepeatable] is bool isRepeatable)
            {
                if (!isRepeatable)
                {
                    // 移除触发器和任务
                    await scheduler.UnscheduleJob(context.Trigger.Key, cancellationToken);
                    await scheduler.DeleteJob(context.JobDetail.Key, cancellationToken);

                    Log.Logger.Debug($"task {context.JobDetail.Key} removed");
                }
            }
        }
    }
}
