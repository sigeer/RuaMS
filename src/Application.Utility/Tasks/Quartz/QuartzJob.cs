using Quartz;
using Serilog;

namespace Application.Utility.Tasks
{
    public class QuartzJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var type = context.JobDetail.JobDataMap[JobDataKeys.Data];

            if (type is AbstractRunnable r)
                r.run();
            else if (type is AsyncAbstractRunnable asyncR)
                await asyncR.RunAsync();
            else if (type is Func<Task> t)
                await t();
            else
                Log.Logger.Error($"TaskJob Invalid, {(type == null ? "null" : type.GetType().Name)}");
        }
    }
}
