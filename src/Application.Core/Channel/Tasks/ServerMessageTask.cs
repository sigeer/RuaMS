using Application.Core.Channel.Commands;

namespace Application.Core.Channel.ServerData
{
    public class ServerMessageTask : ActorTask<WorldChannelServer>
    {
        readonly WorldChannelServer _server;

        public ServerMessageTask(WorldChannelServer server) : base(server, nameof(ServerMessageManager), TimeSpan.FromSeconds(10))
        {
            this._server = server;
        }


        protected override void HandleRun()
        {
            _server.Broadcast(w =>
            {
                w.ServerMessageManager.HandleRun();
            });
        }

    }
}
