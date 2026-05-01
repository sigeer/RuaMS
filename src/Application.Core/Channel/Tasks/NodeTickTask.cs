namespace Application.Core.Channel.Tasks
{
    public class NodeTickTask : TaskBase
    {
        readonly WorldChannelServer _server;

        public NodeTickTask(WorldChannelServer server) : base(nameof(NodeTickTask),
            TimeSpan.FromMilliseconds(YamlConfig.config.server.MOB_STATUS_MONITOR_PROC),
            TimeSpan.FromMilliseconds(YamlConfig.config.server.MOB_STATUS_MONITOR_PROC))
        {
            this._server = server;
        }

        protected override void HandleRun()
        {
            _server.UpdateServerTime(YamlConfig.config.server.MOB_STATUS_MONITOR_PROC);
            _server.Send(n =>
            {
                n.OnTick(_server.getCurrentTime());
            });

        }

    }
}
