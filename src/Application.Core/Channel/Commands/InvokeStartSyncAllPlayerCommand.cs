namespace Application.Core.Channel.Commands
{
    internal class InvokeStartSyncAllPlayerCommand : IWorldChannelAsyncCommand
    {
        public string Name => nameof(InvokeStartSyncAllPlayerCommand);

        bool _saveDB;

        public InvokeStartSyncAllPlayerCommand(bool saveDB)
        {
            _saveDB = saveDB;
        }

        public async Task Execute(WorldChannel ctx)
        {
            List<SyncProto.PlayerSaveDto> list = [];
            foreach (var player in ctx.getPlayerStorage().getAllCharacters())
            {
                list.Add(ctx.NodeService.DataService.Deserialize(player));
            }
            await ctx.Node.Transport.BatchSyncPlayer(list, _saveDB);
        }
    }
}
