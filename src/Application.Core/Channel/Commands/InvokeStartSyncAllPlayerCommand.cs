namespace Application.Core.Channel.Commands
{
    internal class InvokeStartSyncAllPlayerCommand : IWorldChannelCommand
    {
        bool _saveDB;

        public InvokeStartSyncAllPlayerCommand(bool saveDB)
        {
            _saveDB = saveDB;
        }

        public void Execute(WorldChannel ctx)
        {
            List<SyncProto.PlayerSaveDto> list = [];
            foreach (var player in ctx.getPlayerStorage().getAllCharacters())
            {
                list.Add(ctx.NodeService.DataService.Deserialize(player));
            }
            ctx.NodeActor.Send(new InvokeSyncAllPlayerCommand(_saveDB, new DistributeSessionDataWrapper<int, SyncProto.PlayerSaveDto>(ctx.Id, list)));
        }
    }
}
