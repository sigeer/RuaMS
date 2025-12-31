using Application.Core.Channel.HostExtensions;
using Application.Module.Maker.Channel;
using Application.Utility;
using Serilog;
using Serilog.Events;
using System.Text;
using Yitter.IdGenerator;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Configuration.AddEnvironmentVariables(AppSettingKeys.EnvPrefix);
YitIdHelper.SetIdGenerator(new IdGeneratorOptions(builder.Configuration.GetValue<ushort>(AppSettingKeys.LongIdSeed)));

// 日志配置
Log.Logger = new LoggerConfiguration()
#if !DEBUG
    .MinimumLevel.Information()
#else
    .MinimumLevel.Debug()
#endif
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Quartz", LogEventLevel.Warning)
    .MinimumLevel.Override("Grpc", LogEventLevel.Warning)
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

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.AddChannelServer();
builder.Services.AddMakerChannel();

var app = builder.Build();

app.UseChannelServer();


//var p1 = await app.Services.GetRequiredService<ServiceEndpointResolver>().GetEndpointsAsync("http://ruams-master", CancellationToken.None);
//var p2 = await app.Services.GetRequiredService<ServiceEndpointResolver>().GetEndpointsAsync("http://_grpc.ruams-master", CancellationToken.None);
app.Run();