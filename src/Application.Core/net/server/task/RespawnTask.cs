using server.maps;

namespace net.server.task;

/**
 * @author Resinate
 */
public class RespawnTask : AbstractRunnable
{

    public override void HandleRun()
    {
        foreach (var ch in Server.getInstance().getAllChannels())
        {
            var ps = ch.getPlayerStorage();
            if (ps != null)
            {
                if (ps.getAllCharacters().Count() > 0)
                {
                    MapManager mapManager = ch.getMapFactory();
                    if (mapManager != null)
                    {
                        mapManager.updateMaps();
                    }
                }
            }
        }
    }
}
