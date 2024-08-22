using net.server.channel;
using server.maps;

namespace net.server.task;

/**
 * @author Resinate
 */
public class RespawnTask : AbstractRunnable
{

    public override void HandleRun()
    {
        foreach (Channel ch in Server.getInstance().getAllChannels())
        {
            PlayerStorage ps = ch.getPlayerStorage();
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
