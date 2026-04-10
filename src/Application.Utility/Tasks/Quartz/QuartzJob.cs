using Quartz;
using Serilog;

namespace Application.Utility.Tasks
{
    [DisallowConcurrentExecution]
    public class QuartzJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var type = context.JobDetail.JobDataMap[JobDataKeys.Data];

            if (type is AbstractRunnable r)
                r.run();
            else if (type is AsyncAbstractRunnable asyncR)
                return asyncR.RunAsync();
            else if (type is Func<Task> t)
                return t();
            else
                Log.Logger.Error($"TaskJob Invalid, {(type == null ? "null" : type.GetType().Name)}");
            return Task.CompletedTask;
        }
    }
}
