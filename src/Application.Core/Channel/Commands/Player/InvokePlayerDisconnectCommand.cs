namespace Application.Core.Channel.Commands
{
    internal class InvokePlayerDisconnectCommand : IWorldChannelCommand
    {
        int _chrId;

        public InvokePlayerDisconnectCommand(int chrId)
        {
            _chrId = chrId;
        }

        public void Execute(WorldChannel ctx)
        {
            var chr = ctx.getPlayerStorage().GetCharacterClientById(_chrId);
            if (chr != null)
            {
                chr.Client.Disconnect(false, false);
            }

        }
    }
}
