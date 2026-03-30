namespace Application.Core.Channel.Commands
{
    internal class InvokeSyncAllPlayerCommand : IChannelCommand
    {
        bool _saveDB;
        DistributeSessionDataWrapper<int, SyncProto.PlayerSaveDto> _data;

        public InvokeSyncAllPlayerCommand(bool saveDB, DistributeSessionDataWrapper<int, SyncProto.PlayerSaveDto> data)
        {
            _saveDB = saveDB;
            _data = data;
        }

        public void Execute(WorldChannelServer ctx)
        {
            if (ctx.SyncPlayerSession == null)
                ctx.SyncPlayerSession = ctx.CreateSyncPlayerSession();

            if (ctx.SyncPlayerSession.CompleteChunk(_data))
            {
                _ = ctx.Transport.BatchSyncPlayer(ctx.SyncPlayerSession.Chunks, _saveDB);
                ctx.SyncPlayerSession = null;
            }
        }
    }
}
