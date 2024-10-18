using Application.Core.constants;
using Quartz;

namespace Application.Core.scripting.Event.jobs
{
    public class AbstractRunnableJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var type = context.JobDetail.JobDataMap[JobDataKeys.Data];

            if (type is AbstractRunnable r)
                r.run();
            else
                Log.Logger.Error($"TaskJob Invalid, {(type == null ? "null" : type.GetType().Name)}");
            return Task.CompletedTask;
        }
    }
}
