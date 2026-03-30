namespace Application.Core.Channel.Commands
{
    internal class RequestRemoveDoorCommand : IWorldChannelCommand
    {
        int _ownerId;
        public RequestRemoveDoorCommand(int ownerId)
        {
            _ownerId = ownerId;
        }

        public void Execute(WorldChannel ctx)
        {
            _ = ctx.Node.Transport.SendRemoveDoor(_ownerId);
        }
    }

    public class InvokeRemoveDoorCommand : IWorldChannelCommand
    {
        int _ownerId;
        public InvokeRemoveDoorCommand(int ownerId)
        {
            _ownerId = ownerId;
        }

        public void Execute(WorldChannel ctx)
        {
            if (ctx.PlayerDoors.Remove(_ownerId, out var door) && door != null)
            {
                door.Destroy();
            }
        }
    }
}
