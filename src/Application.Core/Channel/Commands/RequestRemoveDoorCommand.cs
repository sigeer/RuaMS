namespace Application.Core.Channel.Commands
{
    internal class RequestRemoveDoorCommand : IWorldChannelCommand
    {
        int _ownerId;
        public RequestRemoveDoorCommand(int ownerId)
        {
            _ownerId = ownerId;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _ = ctx.WorldChannel.Node.Transport.SendRemoveDoor(_ownerId);
        }
    }

    public class InvokeRemoveDoorCommand : IWorldChannelCommand
    {
        int _ownerId;
        public InvokeRemoveDoorCommand(int ownerId)
        {
            _ownerId = ownerId;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (ctx.WorldChannel.PlayerDoors.Remove(_ownerId, out var door) && door != null)
            {
                door.Destroy();
            }
        }
    }
}
