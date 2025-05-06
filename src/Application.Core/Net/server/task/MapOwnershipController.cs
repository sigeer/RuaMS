using Application.Core.Game.Maps;
using Application.Core.Game.TheWorld;

namespace net.server.task;


public class MapOwnershipController : TimelyControllerBase
{

    private HashSet<IMap> ownedMaps = new();
    public MapOwnershipController(IWorldChannel world) : base("MapOwnershipController", TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20))
    {
    }

    public void registerOwnedMap(IMap map)
    {
        ownedMaps.Add(map);
    }

    public void unregisterOwnedMap(IMap map)
    {
        ownedMaps.Remove(map);
    }

    protected override void HandleRun()
    {
        if (ownedMaps.Count > 0)
        {
            List<IMap> ownedMapsList;

            lock (ownedMaps)
            {
                ownedMapsList = new(ownedMaps);
            }

            foreach (var map in ownedMapsList)
            {
                map.checkMapOwnerActivity();
            }
        }
    }

}
