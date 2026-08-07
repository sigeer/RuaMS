using Application.Core.Login;
using Application.Host.Middlewares;
using Application.Host.Services;
using Application.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using System.Text;
using Yitter.IdGenerator;
using Application.Core.Channel.HostExtensions;
using Mapster;
using Google.Protobuf.Collections;




#if IsStandalone
using Application.Core.Channel.InProgress;
#endif

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddEnvironmentVariables(AppSettingKeys.EnvPrefix);

    // 支持GBK
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    Console.OutputEncoding = Encoding.UTF8;

    YitIdHelper.SetIdGenerator(new IdGeneratorOptions(builder.Configuration.GetValue<ushort>(AppSettingKeys.LongIdSeed)));

    // 日志配置
    var logTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{Category}] {Message:lj}{NewLine}{Exception}";
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
        .Enrich.WithProperty("Category", "RuaMS")
        .WriteTo.Console(outputTemplate: logTemplate)
        .WriteTo.Map(
            keySelector: logEvent =>
                logEvent.Properties.TryGetValue("Category", out var category) ? category?.ToString()?.Trim('"') : "Default",
                configure: (category, writeTo) =>
                    writeTo.Logger(
                        lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Error)
                        .WriteTo.Async(a => a.File($"logs/AllError/Error-.txt", rollingInterval: RollingInterval.Day, outputTemplate: logTemplate))
                    )
                    .WriteTo.Logger(lg => lg.WriteTo.Async(a => a.File($"logs/{category}/All-.txt", rollingInterval: RollingInterval.Day, outputTemplate: logTemplate)))
    )
    .CreateLogger();

    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog();

    builder.Services.AddLoginServer(builder.Configuration);

#if IsStandalone
    builder.AddChannelServerInProgress();
#endif

    TypeAdapterConfig.GlobalSettings.Compile();

    builder.AddApiService();

    builder.AddServiceDefaults();

    var app = builder.Build();
#if IsStandalone
    app.UseChannelServer();
#endif

    app.UseApiService();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application failed to start: {ex}");
}
