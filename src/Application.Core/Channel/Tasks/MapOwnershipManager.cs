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
    protected override void HandleRun()
    {
        _server.Broadcast(w =>
        {
            w.MapOwnershipManager.HandleRun();
        });
    }
}
