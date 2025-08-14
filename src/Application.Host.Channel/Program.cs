using Application.Core.Channel;
using Application.Module.Duey.Channel;
using Application.Module.Maker.Channel;
using Application.Module.PlayerNPC.Channel;
using Serilog.Events;
using Serilog;
using System.Text;
using Yitter.IdGenerator;
using Application.Shared.Servers;
using Application.Utility;
using Application.Protos;
using ServiceProto;
using Application.Host.Channel;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddChannelServer();
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<LoggingInterceptor>();
});

builder.Services.AddDueyChannel();
builder.Services.AddMakerChannel();
builder.Services.AddPlayerNPCChannel();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(builder.Configuration.GetValue<int>(AppSettingKeys.GrpcPort), listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

var app = builder.Build();

var bootstrap = app.Services.GetServices<IServerBootstrap>();
foreach (var item in bootstrap)
{
    item.ConfigureHost(app);
}
app.MapGrpcService<WorldChannelGrpcServer>();

app.Run();