using Application.Core.Channel.Commands.Channel;

namespace Application.Core.Channel.ServerData
{
    public class MountTirednessTask : TaskBase
    {
        readonly WorldChannelServer _server;

        public MountTirednessTask(WorldChannelServer server) : base($"{server.ServerName}_{nameof(MountTirednessManager)}", TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
        {
            this._server = server;
        }

        protected override void HandleRun()
        {
            _server.PushChannelCommand(new InvokeMoutTirednessCommand());
        }
    }
}
