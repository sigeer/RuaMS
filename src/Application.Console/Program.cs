using Application.Core.Compatible;
using Application.Core.scripting.Event.jobs;
using net.server;
using Quartz.Impl;
using Serilog;
using Serilog.Events;

var factory = new StdSchedulerFactory();
SchedulerManage.Scheduler = await factory.GetScheduler();
SchedulerManage.Scheduler.ListenerManager.AddJobListener(new JobCompleteListener());

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Quartz", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Map(
        keySelector: logEvent =>
            logEvent.Properties.TryGetValue("Category", out var category) ? category?.ToString()?.Trim('"') : "Default",
            configure: (category, writeTo) =>
                writeTo.Logger(
                    lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Error).WriteTo.Async(a => a.File($"logs/AllError/Error-.txt", rollingInterval: RollingInterval.Day))
                )
                .WriteTo.Logger(lg => lg.WriteTo.Async(a => a.File($"logs/{category}/All-.txt", rollingInterval: RollingInterval.Day)))
         )
    .CreateLogger();

try
{
    await Server.getInstance().Start();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    Log.CloseAndFlush();
}