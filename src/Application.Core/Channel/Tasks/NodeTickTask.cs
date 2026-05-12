namespace Application.Core.Channel.Tasks
{
    public class NodeTickTask : ActorTask<WorldChannelServer>
    {
        readonly WorldChannelServer _server;

        public NodeTickTask(WorldChannelServer server) : base(server, nameof(NodeTickTask),
            TimeSpan.FromMilliseconds(YamlConfig.config.server.MOB_STATUS_MONITOR_PROC))
        {
            this._server = server;
        }

        protected override void HandleRun()
        {
            _server.UpdateServerTime(YamlConfig.config.server.MOB_STATUS_MONITOR_PROC);
            _server.OnTick(_server.getCurrentTime());
        }

    }
}
