using Application.Core.Channel.Commands;

namespace Application.Core.Channel.Tasks
{
    public class DisposeCheckTask : AbstractRunnable
    {
        readonly WorldChannelServer _server;
        public DisposeCheckTask(WorldChannelServer server) : base(
            $"{server.ServerName}_{nameof(DisposeCheckTask)}")
        {
            _server = server;
        }

        public override void HandleRun()
        {
            _server.PushChannelCommand(new MapDisposeCheckCommand());
        }
    }
}
