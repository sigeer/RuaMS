namespace Application.Core.Channel.Commands
{
    internal class InvokeSyncPlayerShopCommand : IChannelAsyncCommand
    {
        public string Name => nameof(InvokeSyncPlayerShopCommand);
        List<ItemProto.SyncPlayerShopRequest> _data;

        public InvokeSyncPlayerShopCommand(List<ItemProto.SyncPlayerShopRequest> data)
        {
            _data = data;
        }

        public async Task Execute(WorldChannelServer ctx)
        {
            var request = new ItemProto.BatchSyncPlayerShopRequest();
            request.List.AddRange(_data);
            await ctx.Transport.BatchSyncPlayerShop(request);
        }
    }
}
