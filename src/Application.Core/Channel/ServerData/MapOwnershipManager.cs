using Application.Core.Game.Maps;

namespace Application.Core.Channel.ServerData;

public class MapOwnershipManager
{
    private HashSet<IMap> ownedMaps = new();
    readonly WorldChannel _server;

    public MapOwnershipManager(WorldChannel server)
    {
        _server = server;
    }
    public void HandleRun()
    {
        foreach (var map in ownedMaps)
        {
            map.checkMapOwnerActivity();
        }
    }

    public void RegisterOwnedMap(IMap map)
    {
        ownedMaps.Add(map);
    }

    public void UnregisterOwnedMap(IMap map)
    {
        ownedMaps.Remove(map);
    }
}
