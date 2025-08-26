namespace Application.Core.Channel.Tasks
{
    public class DisposeCheckTask : AbstractRunnable
    {
        readonly WorldChannelServer _server;
        public DisposeCheckTask(WorldChannelServer server) : base(
            $"ChannelServer:{server.ServerName}_{nameof(DisposeCheckTask)}")
        {
            _server = server;
        }

        public override void HandleRun()
        {
            foreach (var ch in _server.Servers.Values)
            {
                ch.getMapFactory().CheckActive();
            }
        }
    }
}
