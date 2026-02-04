using Application.Core.Channel;
using Application.Core.Channel.Commands;

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
        _server.PushChannelCommand(new TimeoutCheckCommand());
    }
}
