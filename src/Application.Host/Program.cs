using Application.Core.Login;
using Application.Host.Middlewares;
using Application.Host.Services;
using Application.Module.Duey.Master;
using Application.Module.ExpeditionBossLog.Master;
using Application.Module.Maker.Master;
using Application.Module.PlayerNPC.Master;
using Application.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using System.Text;
using Yitter.IdGenerator;
using Application.Core.Channel.HostExtensions;

#if IsStandalone
using Application.Core.Channel.InProgress;
#endif

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddEnvironmentVariables(AppSettingKeys.EnvPrefix);

    // 支持GBK
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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

    builder.Services.AddLoginServer(builder.Configuration);
    builder.Services.AddDueyMaster();
    builder.Services.AddExpeditionBossLogMaster();
    builder.Services.AddMakerMaster();
    builder.Services.AddPlayerNPCMaster();

#if IsStandalone
    builder.AddChannelServerInProgress();
#endif

    builder.WebHost.ConfigureKestrel(options =>
    {
        if (builder.Configuration.GetValue<bool>(AppSettingKeys.EnableOpenApi))
        {
            options.ListenAnyIP(builder.Configuration.GetValue<int>(AppSettingKeys.OpenApiPort), listenOptions =>
            {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
            });
        }

        if (builder.Configuration.GetValue<bool>(AppSettingKeys.UseExtraChannel))
        {
            options.ListenAnyIP(builder.Configuration.GetValue<int>(AppSettingKeys.GrpcPort), listenOptions =>
            {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
            });
        }
    });

    if (builder.Configuration.GetValue<bool>(AppSettingKeys.EnableOpenApi))
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("cors", p =>
            {
                var allowedHost = builder.Configuration.GetValue<string>("AllowedHosts");
                if (string.IsNullOrEmpty(allowedHost) || allowedHost == "*")
                    p.SetIsOriginAllowed(_ => true);
                else
                    p.WithOrigins(allowedHost.Split(","));

                p
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            });
        });

        //builder.Services.AddQuartz(o => o.AddJobListener(new JobCompleteListener()));
        //builder.Services.AddSingleton<TimerManager>();

        builder.Services.AddScoped<AuthService>();

        builder.Services.AddAuthentication(s =>
        {
            s.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            s.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            s.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                ValidateIssuer = true,
                ValidIssuer = "ruams",

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthService.GetAuthCode())),

                ValidateAudience = false
            };
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    //Token expired
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers["Token-Expired"] = "true";
                    }

                    return Task.CompletedTask;
                },
            };
        });

        // Api
        builder.Services.AddControllers(o =>
            o.Filters.Add<DataWrapperFilter>()
        );

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddOpenApi();
    }

    var app = builder.Build();
#if IsStandalone
    app.UseChannelServer();
#endif

    if (builder.Configuration.GetValue<bool>(AppSettingKeys.EnableOpenApi))
    {
        var authCode = AuthService.GetAuthCode();
        Log.Logger.Information("授权码>>：[" + authCode + "]");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference(options =>
            {
                options.AddServer($"http://localhost:{builder.Configuration.GetValue<int>(AppSettingKeys.OpenApiPort)}");
            });
            app.MapOpenApi();
        }

        app.UseCors("cors");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application failed to start: {ex}");
}