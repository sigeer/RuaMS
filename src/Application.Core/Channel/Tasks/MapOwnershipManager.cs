namespace Application.Core.Channel.ServerData;

public class MapOwnershipTask : TaskBase
{
    readonly WorldChannelServer _server;

    public MapOwnershipTask(WorldChannelServer server)
        : base(nameof(MapOwnershipManager),
              TimeSpan.FromSeconds(20),
              TimeSpan.FromSeconds(20))
    {
        _server = server;
    }
    protected override async Task HandleRun()
    {
        await _server.BroadcastAsync(async w =>
        {
            await w.MapOwnershipManager.HandleRun();
        });
    }
}
