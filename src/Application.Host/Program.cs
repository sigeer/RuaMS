using Application.Core;
using Application.Core.Compatible;
using Application.Core.scripting.Event.jobs;
using Application.EF;
using Application.Host;
using Application.Host.Middlewares;
using Application.Host.Models;
using Application.Host.Services;
using Application.Utility.Configs;
using Microsoft.EntityFrameworkCore;
using Quartz.Impl;
using Serilog;
using Serilog.Events;
using System.Text;

Environment.SetEnvironmentVariable("wz-path", "D:\\Cosmic\\wz");

var builder = WebApplication.CreateBuilder(args);

// 支持GBK
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
GlobalTools.Encoding = Encoding.GetEncoding("GBK");

// 日志配置
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
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

builder.Services.AddLogging(o => o.AddSerilog());

// 数据库配置
builder.Services.AddDbContext<DBContext>(o => o.UseMySQL(YamlConfig.config.server.DB_CONNECTIONSTRING));

var factory = new StdSchedulerFactory();
SchedulerManage.Scheduler = await factory.GetScheduler();
SchedulerManage.Scheduler.ListenerManager.AddJobListener(new JobCompleteListener());
//builder.Services.AddQuartz(o => o.AddJobListener(new JobCompleteListener()));
//builder.Services.AddSingleton<TimerManager>();

// 游戏服务
builder.Services.AddHostedService<GameHost>();

builder.Services.AddScoped<DropdataService>();
builder.Services.AddAutoMapper(typeof(DtoMapper).Assembly);
// Api
builder.Services.AddControllers(o =>
    o.Filters.Add<DataWrapperFilter>()
);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
