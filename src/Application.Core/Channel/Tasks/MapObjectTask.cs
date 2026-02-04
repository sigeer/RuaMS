using Application.Core.Channel.Commands.Channel;

namespace Application.Core.Channel.Tasks
{
    public class MapObjectTask : TaskBase
    {
        readonly WorldChannelServer _server;
        public MapObjectTask(WorldChannelServer server) : base($"{server.ServerName}_{nameof(MapObjectTask)}", TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
        {
            _server = server;
        }

        protected override void HandleRun()
        {
            _server.PushChannelCommand(new InvokeMapObjectClearCommand());
        }
    }
}
