using Quartz.Impl;

namespace server;

public class TimerManager
{

    public static async Task<ITimerManager> InitializeAsync(TaskEngine engine)
    {
        switch (engine)
        {
            case TaskEngine.Task:
                var instance = new TaskTimerManager();
                await instance.Start();
                return instance;
            case TaskEngine.Quartz:
                var factory = new StdSchedulerFactory();
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


    //public static void purge()
    //{
    //    //Yay?
    //    Server.getInstance().forceUpdateCurrentTime();
    //}
}
