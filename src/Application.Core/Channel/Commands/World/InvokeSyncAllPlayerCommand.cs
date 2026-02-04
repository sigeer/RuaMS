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

        public void Execute(ChannelNodeCommandContext ctx)
        {
            if (ctx.Server.SyncPlayerSession == null)
                ctx.Server.SyncPlayerSession = ctx.Server.CreateSyncPlayerSession();

            if (ctx.Server.SyncPlayerSession.CompleteChunk(_data))
            {
                _ = ctx.Server.Transport.BatchSyncPlayer(ctx.Server.SyncPlayerSession.Chunks, _saveDB);
                ctx.Server.SyncPlayerSession = null;
            }
        }
    }
}
