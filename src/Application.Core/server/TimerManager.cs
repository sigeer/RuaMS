using net.server;
using Quartz.Impl;

namespace server;

public class TimerManager
{
    private static Lazy<ITimerManager>? instance = null;

    public static ITimerManager getInstance()
    {
        return instance!.Value;
    }

    public static async Task InitializeAsync(TaskEngine engine)
    {
        if (SchedulerManager.TaskScheduler.Count > 0)
            throw new BusinessFatalException("还有未停止的任务");

        switch (engine)
        {
            case TaskEngine.Task:
                instance = new Lazy<ITimerManager>(new TaskTimerManager());
                break;
            case TaskEngine.Quartz:
                var factory = new StdSchedulerFactory();
                QuartzSchedulerManager.Scheduler = await factory.GetScheduler();
                QuartzSchedulerManager.Scheduler.ListenerManager.AddJobListener(new JobCompleteListener());
                QuartzSchedulerManager.Scheduler.ListenerManager.AddSchedulerListener(new MySchedulerListener());
                instance = new Lazy<ITimerManager>(new QuartzTimerManager(QuartzSchedulerManager.Scheduler));
                break;
            default:
                throw new BusinessFatalException("不支持的任务引擎 " + engine);
        }
    }


    public static void purge()
    {
        //Yay?
        Server.getInstance().forceUpdateCurrentTime();
    }
}
