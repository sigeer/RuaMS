using Application.Core.Channel;
using Application.Core.Channel.Commands;

namespace net.server.task;

/**
 * @author Shavit
 */
public class TimeoutTask : ActorTask<WorldChannelServer>
{
    public TimeoutTask(WorldChannelServer server): base(server, nameof(TimeoutTask), TimeSpan.FromSeconds(10))
    {
    }

    protected override void HandleRun()
    {
        _actor.Broadcast(w =>
        {
            var chars = w.getPlayerStorage().getAllCharacters();
            foreach (var chr in chars)
            {
                if (w.Node.GetCurrentTimeDateTimeOffset() - chr.getClient().LastPacket > TimeSpan.FromMilliseconds(YamlConfig.config.server.TIMEOUT_DURATION))
                {
                    Log.Logger.Information("Chr {CharacterName} auto-disconnected due to inactivity", chr.getName());
                    chr.getClient().Disconnect(true, chr.getCashShop().isOpened());
                }
            }
        });
    }
}
