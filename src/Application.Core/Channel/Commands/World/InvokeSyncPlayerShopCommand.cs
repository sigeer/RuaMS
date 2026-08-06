namespace Application.Core.Channel.Commands
{
    internal class InvokeSyncPlayerShopCommand : IChannelAsyncCommand
    {
        public string Name => nameof(InvokeSyncPlayerShopCommand);
        List<ProtoService.SyncPlayerShopRequest> _data;

        public InvokeSyncPlayerShopCommand(List<ProtoService.SyncPlayerShopRequest> data)
        {
            _data = data;
        }

        public async Task Execute(WorldChannelServer ctx)
        {
            var request = new ProtoService.BatchSyncPlayerShopRequest();
            request.List.AddRange(_data);
            await ctx.Transport.BatchSyncPlayerShop(request);
        }
    }
}
