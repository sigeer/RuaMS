using Application.Utility.Exceptions;
using Quartz.Impl;
using System.Collections.Specialized;

namespace Application.Utility.Tasks
{
    public class TimerManagerFactory
    {
        public static async Task<ITimerManager> InitializeAsync(TaskEngine engine, string schedulerName)
        {
            switch (engine)
            {
                case TaskEngine.Task:
                    var instance = new TaskTimerManager();
                    await instance.Start();
                    return instance;
                case TaskEngine.Quartz:
                    var properties = new NameValueCollection
                {
                    { "quartz.scheduler.instanceName", schedulerName }
                };
                    var factory = new StdSchedulerFactory(properties);
                    var scheduler = await factory.GetScheduler();

                    var quratz = new QuartzTimerManager(scheduler);
                    scheduler.ListenerManager.AddJobListener(new JobCompleteListener());
                    scheduler.ListenerManager.AddSchedulerListener(new MySchedulerListener(quratz));

                    await quratz.Start();
                    return quratz;
                default:
                    throw new BusinessFatalException("不支持的任务引擎 " + engine);
            }
        }
    }
}
