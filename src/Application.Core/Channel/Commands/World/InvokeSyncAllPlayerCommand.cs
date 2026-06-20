namespace Application.Core.Channel.Commands
{
    internal class InvokeSyncAllPlayerCommand : IChannelAsyncCommand
    {
        public string Name => nameof(InvokeSyncAllPlayerCommand);
        bool _saveDB;
        DistributeSessionDataWrapper<int, SyncProto.PlayerSaveDto> _data;

        public InvokeSyncAllPlayerCommand(bool saveDB, DistributeSessionDataWrapper<int, SyncProto.PlayerSaveDto> data)
        {
            _saveDB = saveDB;
            _data = data;
        }

        public async Task Execute(WorldChannelServer ctx)
        {
            if (ctx.SyncPlayerSession == null)
                ctx.SyncPlayerSession = ctx.CreateSyncPlayerSession();

            if (ctx.SyncPlayerSession.CompleteChunk(_data))
            {
                await ctx.Transport.BatchSyncPlayer(ctx.SyncPlayerSession.Chunks, _saveDB);
                ctx.SyncPlayerSession = null;
            }
        }
    }
}
