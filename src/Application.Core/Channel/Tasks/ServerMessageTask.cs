using Application.Core.Channel.Commands;

namespace Application.Core.Channel.ServerData
{
    public class ServerMessageTask : TaskBase
    {
        readonly WorldChannelServer _server;

        public ServerMessageTask(WorldChannelServer server) : base($"ChannelServer:{server.ServerName}_{nameof(ServerMessageManager)}", TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10))
        {
            this._server = server;
        }


        protected override void HandleRun()
        {
            _server.PushChannelCommand(new ServerMessageCommand());
        }

    }
}
