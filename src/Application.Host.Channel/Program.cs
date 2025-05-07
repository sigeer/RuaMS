using Application.Core.Channel;
using Application.Core.Game.TheWorld;
using Application.Core.Servers;
using Application.Host.Channel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IWorldChannel>(o => new WorldChannel(new ChannelServerConfig { }, new RemoteChannelServerTransport()));
// Add services to the container.
builder.Services.AddHostedService<ChannelHost>();

var app = builder.Build();

// Configure the HTTP request pipeline.


app.Run();
