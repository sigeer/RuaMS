namespace Application.Core.Channel.ServerData;

public class MapOwnershipTask : ActorTask<WorldChannelServer>
{
    readonly WorldChannelServer _server;

    public MapOwnershipTask(WorldChannelServer server)
        : base(server, nameof(MapOwnershipManager),
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
