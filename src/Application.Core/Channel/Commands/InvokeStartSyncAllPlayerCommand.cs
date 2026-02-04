namespace Application.Core.Channel.Commands
{
    internal class InvokeStartSyncAllPlayerCommand : IWorldChannelCommand
    {
        bool _saveDB;

        public InvokeStartSyncAllPlayerCommand(bool saveDB)
        {
            _saveDB = saveDB;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            List<SyncProto.PlayerSaveDto> list = [];
            foreach (var player in ctx.WorldChannel.getPlayerStorage().getAllCharacters())
            {
                list.Add(ctx.WorldChannel.NodeService.DataService.Deserialize(player));
            }
            ctx.WorldChannel.NodeActor.Post(new InvokeSyncAllPlayerCommand(_saveDB, new DistributeSessionDataWrapper<int, SyncProto.PlayerSaveDto>(ctx.WorldChannel.Id, list)));
        }
    }
}
