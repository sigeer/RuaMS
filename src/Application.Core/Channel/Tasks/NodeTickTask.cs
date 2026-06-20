using Application.Core.Channel.Actor;
using Application.Core.Game.Maps;
using Application.Utility.Pipeline;

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

        protected override async Task HandleRun()
        {
            _server.UpdateServerTime(YamlConfig.config.server.MOB_STATUS_MONITOR_PROC);
            await _server.OnTick(_server.getCurrentTime());
        }
    }

    public class MapTickCommand : MapDelegateCommand, IIgnoreActivityCommand
    {
        public MapTickCommand(Action<IMap> func) : base(func)
        {
        }
    }
    public class ChannelTickCommand : ChannelDelegateCommand, IIgnoreActivityCommand
    {
        public ChannelTickCommand(Action<WorldChannel> func) : base(func)
        {
        }
    }
    public class NodeTickCommand : NodeDelegateCommand, IIgnoreActivityCommand
    {
        public NodeTickCommand(Action<WorldChannelServer> func) : base(func)
        {
        }
    }
}
