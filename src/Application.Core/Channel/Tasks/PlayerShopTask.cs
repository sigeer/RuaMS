namespace Application.Core.Channel.Tasks
{
    public class PlayerShopTask : AsyncAbstractRunnable
    {
        readonly WorldChannelServer _server;

        public PlayerShopTask(WorldChannelServer server) : base($"ChannelServer:{server.ServerName}_{nameof(PlayerShopTask)}")
        {
            _server = server;
        }

        public override async Task RunAsync()
        {
            var request = new ItemProto.BatchSyncPlayerShopRequest();
            foreach (var ch in _server.Servers.Values)
            {
                var r = ch.PlayerShopManager.CheckExpired();
                request.List.AddRange(r);
            }
            if (request.List.Count == 0)
                return;

            await _server.Transport.BatchSyncPlayerShop(request);
        }
    }
}
