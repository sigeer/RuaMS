using Application.Core.Channel;

namespace net.server.task;

/**
 * @author Shavit
 */
public class TimeoutTask : BaseTask
{
    public override void HandleRun()
    {
        var time = DateTimeOffset.UtcNow;
        var chars = wserv.getPlayerStorage().GetAllOnlinedPlayers();
        foreach (var chr in chars)
        {
            if (time - chr.getClient().LastPacket > TimeSpan.FromMilliseconds(YamlConfig.config.server.TIMEOUT_DURATION))
            {
                log.Information("Chr {CharacterName} auto-disconnected due to inactivity", chr.getName());
                chr.getClient().Disconnect(true, chr.getCashShop().isOpened());
            }
        }
    }

    public TimeoutTask(World world) : base(world)
    {
    }
}
