using Application.Server.Channel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHostedService<ChannelHost>();

var app = builder.Build();

// Configure the HTTP request pipeline.


app.Run();