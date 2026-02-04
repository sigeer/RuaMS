using Application.Utility.Pipeline;

namespace Application.Core.Channel.Commands
{
    internal class InvokePetHungerCommand : IWorldChannelCommand
    {
        public void Execute(ChannelCommandContext ctx)
        {
            ctx.WorldChannel.PetHungerManager.HandleRun();
        }
    }
}
