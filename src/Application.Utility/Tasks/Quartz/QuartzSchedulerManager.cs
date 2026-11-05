using Quartz;
using Serilog;

namespace Application.Utility.Tasks
{

    public class MySchedulerListener : ISchedulerListener
    {
        readonly QuartzTimerManager _timerManager;

        public MySchedulerListener(QuartzTimerManager timerManager)
        {
            _timerManager = timerManager;
        }

        public async ValueTask JobDeleted(IScheduler scheduler, JobKey jobKey, CancellationToken cancellationToken)
        {
            if (_timerManager.TaskScheduler.TryRemove(jobKey.Name, out var p) && p is QuartzScheduledFuture data)
            {
                Log.Logger.Debug("结束了一个任务，JobId = {JobId}", jobKey.Name);
                await scheduler.UnscheduleJob(data.TriggerKey, cancellationToken);
            }
        }
    }

    public class JobCompleteListener : IJobListener
    {
        public string Name => "JobCompleteListener";

        public async ValueTask JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
        {
            var scheduler = context.Scheduler;
            if (jobException?.InnerException is OperationCanceledException)
            {
                // 移除触发器和任务
                await scheduler.UnscheduleJob(context.Trigger.Key, cancellationToken);
                await scheduler.DeleteJob(context.JobDetail.Key, cancellationToken);
                return;
            }


            if (context.JobDetail.JobDataMap[JobDataKeys.IsRepeatable] is bool isRepeatable)
            {
                if (!isRepeatable)
                {
                    // 移除触发器和任务
                    await scheduler.UnscheduleJob(context.Trigger.Key, cancellationToken);
                    await scheduler.DeleteJob(context.JobDetail.Key, cancellationToken);
                    return;
                }
            }


            if (jobException != null)
            {
                Log.Logger.Error(jobException, "任务发生错误，JobId = {JobId}, 错误：{Error}", context.JobDetail.Key, jobException.Message);
            }
        }
    }
}
