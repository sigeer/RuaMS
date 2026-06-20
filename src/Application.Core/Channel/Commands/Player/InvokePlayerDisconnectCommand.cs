namespace Application.Core.Channel.Commands
{
    internal class InvokePlayerDisconnectCommand : IWorldChannelAsyncCommand
    {
        public string Name => nameof(InvokePlayerDisconnectCommand);
        int _chrId;

        public InvokePlayerDisconnectCommand(int chrId)
        {
            _chrId = chrId;
        }

        public async Task Execute(WorldChannel ctx)
        {
            var chr = ctx.getPlayerStorage().GetCharacterClientById(_chrId);
            if (chr != null)
            {
                await chr.Client.Disconnect(false, false);
            }

        }
    }
}
