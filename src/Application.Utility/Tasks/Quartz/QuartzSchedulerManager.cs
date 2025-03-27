using Quartz;
using Serilog;

namespace Application.Utility.Tasks
{
    public class QuartzSchedulerManager
    {
        public static IScheduler Scheduler { get; set; } = null!;
    }

    public class MySchedulerListener : ISchedulerListener
    {
        public Task JobAdded(IJobDetail jobDetail, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task JobDeleted(JobKey jobKey, CancellationToken cancellationToken)
        {
            if (SchedulerManager.TaskScheduler.Remove(jobKey.Name, out var p) && p is QuartzScheduledFuture data)
            {
                Log.Logger.Debug("结束了一个任务，JobId = {JobId}", jobKey.Name);
                QuartzSchedulerManager.Scheduler.UnscheduleJob(data.TriggerKey);
            }
            return Task.CompletedTask;
        }

        public Task JobInterrupted(JobKey jobKey, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task JobPaused(JobKey jobKey, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task JobResumed(JobKey jobKey, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task JobScheduled(ITrigger trigger, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task JobsPaused(string jobGroup, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task JobsResumed(string jobGroup, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task JobUnscheduled(TriggerKey triggerKey, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SchedulerError(string msg, SchedulerException cause, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SchedulerInStandbyMode(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SchedulerShutdown(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SchedulerShuttingdown(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SchedulerStarted(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SchedulerStarting(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SchedulingDataCleared(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task TriggerAdded(ITrigger trigger, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task TriggerDeleted(TriggerKey triggerKey, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task TriggerFinalized(ITrigger trigger, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task TriggerPaused(TriggerKey triggerKey, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task TriggerResumed(TriggerKey triggerKey, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task TriggersPaused(string? triggerGroup, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task TriggersResumed(string? triggerGroup, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        // 其他方法省略...
    }
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
