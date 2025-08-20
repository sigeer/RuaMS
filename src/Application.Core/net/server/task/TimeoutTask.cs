using Application.Core.Channel;

namespace net.server.task;

/**
 * @author Shavit
 */
public class TimeoutTask : AbstractRunnable
{
    readonly WorldChannelServer _server;

    public TimeoutTask(WorldChannelServer server)
    {
        _server = server;
    }

    public override void HandleRun()
    {
        var time = DateTimeOffset.UtcNow;
        var chars = _server.PlayerStorage.getAllCharacters();
        foreach (var chr in chars)
        {
            if (time - chr.getClient().LastPacket > TimeSpan.FromMilliseconds(YamlConfig.config.server.TIMEOUT_DURATION))
            {
                log.Information("Chr {CharacterName} auto-disconnected due to inactivity", chr.getName());
                chr.getClient().Disconnect(true, chr.getCashShop().isOpened());
            }
        }
    }
}
