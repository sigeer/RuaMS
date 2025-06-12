using Application.Core.Game.Controllers;
using Application.Core.Game.Maps;

namespace Application.Core.Channel.ServerData;

public class MapOwnershipManager : TimelyControllerBase
{

    private HashSet<IMap> ownedMaps = new();

    public MapOwnershipManager(WorldChannelServer worldChannel)
        : base($"MapOwnershipTask_{worldChannel.ServerName}",
              TimeSpan.FromSeconds(20),
              TimeSpan.FromSeconds(20))
    {
    }
    protected override void HandleRun()
    {
        RunCheckOwnedMapsSchedule();
    }

    public void RegisterOwnedMap(IMap map)
    {
        ownedMaps.Add(map);
    }

    public void UnregisterOwnedMap(IMap map)
    {
        ownedMaps.Remove(map);
    }

    public void RunCheckOwnedMapsSchedule()
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
