/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using Application.Core.constants;
using Application.Core.scripting.Event;
using Application.Core.scripting.Event.jobs;
using net.server;
using Quartz;

namespace server;

public class TimerManager
{
    private static ILogger log = LogFactory.GetLogger(LogType.TimerManager);
    private static Lazy<TimerManager> instance = new Lazy<TimerManager>(new TimerManager(SchedulerManage.Scheduler));

    public static TimerManager getInstance()
    {
        return instance.Value;
    }

    readonly IScheduler _scheduler;
    private TimerManager(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public async Task start()
    {
        if (!_scheduler.IsStarted)
            await _scheduler.Start();
    }

    public void stop()
    {
        if (!_scheduler.IsShutdown)
            _scheduler.Shutdown();
    }

    public void purge()
    {//Yay?
        Server.getInstance().forceUpdateCurrentTime();
    }

    /// <summary>
    /// delay微秒之后执行，并且每隔repeatTime微秒之后执行
    /// </summary>
    /// <param name="r"></param>
    /// <param name="repeatTime"></param>
    /// <param name="delay"></param>
    /// <returns>job id</returns>
    public ScheduledFuture register(AbstractRunnable r, long repeatTime, long? delay = null) =>
        register(r, TimeSpan.FromMilliseconds(repeatTime), delay == null ? null : TimeSpan.FromMilliseconds(delay.Value));

    public ScheduledFuture register(AbstractRunnable r, TimeSpan repeatTime, TimeSpan? delay = null)
    {
        var job = JobBuilder.Create<AbstractRunnableJob>()
            .WithIdentity(r.Name)
            .UsingJobData(new JobDataMap() { { JobDataKeys.Data, r }, { JobDataKeys.IsRepeatable, true } })
            .Build();

        var builder = TriggerBuilder.Create()
                        .WithIdentity(r!.Name);
        if (delay == null)
            builder.StartNow();
        else
            builder.StartAt(DateTimeOffset.Now.Add(delay.Value));

        var trigger = builder
                        .WithSimpleSchedule(x => x
                            .WithInterval(repeatTime) // 设置间隔为1秒
                            .RepeatForever()) // 一直重复执行
                        .Build();

        _scheduler.ScheduleJob(job, trigger).Wait();
        return new ScheduledFuture(r!.Name);
    }

    public ScheduledFuture register(Action r, long repeatTime, long? delay = null) => register(TempRunnable.Parse(r), repeatTime, delay);
    public ScheduledFuture register(Action r, TimeSpan repeatTime, TimeSpan? delay = null) => register(TempRunnable.Parse(r), repeatTime, delay);

    public ScheduledFuture schedule(AbstractRunnable r, TimeSpan delay)
    {
        var job = JobBuilder.Create<AbstractRunnableJob>()
            .WithIdentity(r.Name)
            .UsingJobData(new JobDataMap() { { JobDataKeys.Data, r }, { JobDataKeys.IsRepeatable, false } })
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(r.Name)
            .StartAt(DateTimeOffset.Now.Add(delay))
            .Build();

        _scheduler.ScheduleJob(job, trigger).Wait();
        return new ScheduledFuture(r!.Name);
    }

    public ScheduledFuture schedule(Action r, TimeSpan delay)
    {
        return schedule(TempRunnable.Parse(r), delay);
    }

    public ScheduledFuture schedule(Action r, long delay)
    {
        return schedule(TempRunnable.Parse(r), TimeSpan.FromMilliseconds(delay));
    }

    public ScheduledFuture scheduleAtTimestamp(AbstractRunnable r, DateTimeOffset time)
    {
        return schedule(r, (time - DateTimeOffset.Now));
    }

    public ScheduledFuture scheduleAtTimestamp(Action r, DateTimeOffset time)
    {
        return schedule(TempRunnable.Parse(r), time - DateTimeOffset.Now);
    }

    public bool isShutdown()
    {
        return _scheduler.IsShutdown;
    }

    //private static class LoggingSaveRunnable : Runnable {
    //    Runnable r;

    //    public LoggingSaveRunnable(Runnable r) {
    //        this.r = r;
    //    }

    //    public override void run() {
    //        try {
    //            r.run();
    //        } catch (Throwable t) {
    //            log.error("Error in scheduled task", t);
    //        }
    //    }
    //}
}
