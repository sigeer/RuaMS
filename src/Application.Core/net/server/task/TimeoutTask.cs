

using Application.Core.Game.TheWorld;

namespace net.server.task;



/**
 * @author Shavit
 */
public class TimeoutTask : BaseTask
{
    public override void HandleRun()
    {
        long time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var chars = wserv.getPlayerStorage().getAllCharacters();
        foreach (var chr in chars)
        {
            if (time - chr.getClient().getLastPacket() > YamlConfig.config.server.TIMEOUT_DURATION)
            {
                log.Information("Chr {CharacterName} auto-disconnected due to inactivity", chr.getName());
                chr.getClient().disconnect(true, chr.getCashShop().isOpened());
            }
        }
    }

    public TimeoutTask(IWorld world) : base(world)
    {
    }
}
