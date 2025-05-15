using Application.Core.Channel;
using Application.Host.Channel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<RemoteChannelServerTransport>();
builder.Services.AddChannelServer();
// Add services to the container.
builder.Services.AddHostedService<ChannelHost>();

var app = builder.Build();

// Configure the HTTP request pipeline.


app.Run();
