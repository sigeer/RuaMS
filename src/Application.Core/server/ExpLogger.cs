using Application.Core.EF.Entities;
using net.server;
using System.Collections.Concurrent;

namespace server;

public class ExpLogger
{
    private static ConcurrentQueue<ExpLogRecord> expLoggerQueue = new();
    private static short EXP_LOGGER_THREAD_SLEEP_DURATION_SECONDS = 60;
    private static short EXP_LOGGER_THREAD_SHUTDOWN_WAIT_DURATION_MINUTES = 5;

    static ScheduledFuture? expLoggerSchedule;
    static ExpLogger()
    {
        if (YamlConfig.config.server.USE_EXP_GAIN_LOG)
        {
            startExpLogger();
        }
    }
    public static void putExpLogRecord(ExpLogRecord expLogRecord)
    {
        try
        {
            expLoggerQueue.Enqueue(expLogRecord);
        }
        catch (ThreadInterruptedException e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    //    static private ScheduledExecutorService schdExctr = Executors.newSingleThreadScheduledExecutor(new ThreadFactory()
    //    {
    //        public override Thread newThread(Runnable r)
    //    {
    //        Thread t = new Thread(r);
    //        t.setPriority(Thread.MIN_PRIORITY);
    //        return t;
    //    }
    //});

    private static Action saveExpLoggerToDBRunnable = () =>
    {
        try
        {
            using var dbContext = new DBContext();
            // "INSERT INTO characterexplogs (world_exp_rate, exp_coupon, gained_exp, current_exp, exp_gain_time, charid) VALUES (?, ?, ?, ?, ?, ?)"

            List<ExpLogRecord> drainedExpLogs = new();

            while (expLoggerQueue.TryDequeue(out var item))
            {
                drainedExpLogs.Add(item);
            }
            dbContext.ExpLogRecords.AddRange(drainedExpLogs);
            dbContext.SaveChanges();
        }
        catch (Exception sqle)
        {
            Log.Logger.Error(sqle.ToString());
        }

    };


    private static void startExpLogger()
    {
        expLoggerSchedule = Server.getInstance().GlobalTimerManager.register(saveExpLoggerToDBRunnable,
            TimeSpan.FromSeconds(EXP_LOGGER_THREAD_SLEEP_DURATION_SECONDS),
            TimeSpan.FromSeconds(EXP_LOGGER_THREAD_SLEEP_DURATION_SECONDS));

        AppDomain.CurrentDomain.ProcessExit += (obj, evt) => stopExpLogger();
    }

    private static bool stopExpLogger()
    {
        return expLoggerSchedule?.cancel(true) ?? true;
    }
}
