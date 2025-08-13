using Application.Core.Channel.InProgress;
using Application.Host.Middlewares;
using Application.Host.Models;
using Application.Host.Services;
using Application.Shared.Servers;
using Application.Utility;
using Application.Utility.Configs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using System.Text;
using Yitter.IdGenerator;

// Environment.SetEnvironmentVariable("ms-wz", "D:\\Cosmic\\wz");

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

    builder.AddGameServerInProgress();

    if (YamlConfig.config.server.ENABLE_OPENAPI)
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
        builder.Services.AddScoped<DropdataService>();
        builder.Services.AddScoped<DataService>();
        builder.Services.AddScoped<ServerService>();
        builder.Services.AddScoped<Application.Core.OpenApi.ChannelService>();
        builder.Services.AddAutoMapper(typeof(DtoMapper).Assembly);

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
                ValidIssuer = "cosmic_dotnet",

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

    var bootstrap = app.Services.GetServices<IServerBootstrap>();
    foreach (var item in bootstrap)
    {
        item.ConfigureHost(app);
    }

    if (YamlConfig.config.server.ENABLE_OPENAPI)
    {
        var authCode = AuthService.GetAuthCode();
        Log.Logger.Information("授权码>>：[" + authCode + "]");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference();
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