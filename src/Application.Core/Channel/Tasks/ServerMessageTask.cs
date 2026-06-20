namespace Application.Core.Channel.ServerData
{
    public class ServerMessageTask : TaskBase
    {
        readonly WorldChannelServer _server;

        public ServerMessageTask(WorldChannelServer server) : base(nameof(ServerMessageManager), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10))
        {
            this._server = server;
        }


        protected override async Task HandleRun()
        {
            await _server.BroadcastAsync(async w =>
            {
                await w.ServerMessageManager.HandleRun();
            });
        }

    }
}
