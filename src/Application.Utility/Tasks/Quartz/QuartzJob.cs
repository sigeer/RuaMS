using Quartz;
using Serilog;

namespace Application.Utility.Tasks
{
    [DisallowConcurrentExecution]
    public class QuartzJob : IJob
    {
        public ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken)
        {
            var type = context.JobDetail.JobDataMap[JobDataKeys.Data];

            if (type is AbstractRunnable r)
                r.run();
            else if (type is AsyncAbstractRunnable asyncR)
                return new ValueTask(asyncR.RunAsync());
            else if (type is Func<Task> t)
                return new ValueTask(t());
            else
                Log.Logger.Error($"TaskJob Invalid, {(type == null ? "null" : type.GetType().Name)}");
            return ValueTask.CompletedTask;
        }
    }
}
