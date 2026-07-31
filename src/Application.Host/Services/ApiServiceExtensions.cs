using Application.Host.Middlewares;
using Application.Host.Models;
using Application.Templates.Reader;
using Application.Utility;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

namespace Application.Host.Services
{
    public static class ApiServiceExtensions
    {
        public static void AddApiService(this WebApplicationBuilder builder)
        {

            if (builder.Configuration.GetSection(AppSettingKeys.OpenApiEndpoint).Exists())
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

                builder.Services.AddScoped<AuthService>();
                builder.Services.AddScoped<ServerService>();
                builder.Services.AddScoped<WebUserService>();
                builder.Services.AddScoped<DataIdService>();

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
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthService.IssuerSigningKey)),

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
                ).ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var errors = string.Join('|', context.ModelState
                               .Where(e => e.Value?.Errors?.Count > 0)
                               .SelectMany(e => 
                                   e.Value!.Errors.Select(x => x.ErrorMessage)));

                        return new ObjectResult(new ResponseData<object>(null)
                        {
                            Code = 400,
                            Message = "表单提交失败" + errors,
                        });
                    };
                });

                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddOpenApi(o =>
                {
                    o.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                });
            }
        }

        public static void UseApiService(this WebApplication app)
        {
            if (app.Configuration.GetSection(AppSettingKeys.OpenApiEndpoint).Exists())
            {
                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.MapScalarApiReference(options =>
                    {
                        options.AddServer(app.Configuration.GetValue<string>(AppSettingKeys.OpenApiEndpoint));
                    });
                    app.MapOpenApi();
                }

                app.UseCors("cors");

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();
            }
        }
    }
}
