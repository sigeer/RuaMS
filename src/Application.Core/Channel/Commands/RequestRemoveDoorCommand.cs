namespace Application.Core.Channel.Commands
{
    internal class RequestRemoveDoorCommand : IWorldChannelAsyncCommand
    {
        public string Name => nameof(RequestRemoveDoorCommand);
        int _ownerId;
        public RequestRemoveDoorCommand(int ownerId)
        {
            _ownerId = ownerId;
        }

        public Task Execute(WorldChannel ctx)
        {
            return ctx.Node.Transport.SendRemoveDoor(_ownerId);
        }
    }

    public class InvokeRemoveDoorCommand : IWorldChannelAsyncCommand
    {
        public string Name => nameof(InvokeRemoveDoorCommand);
        int _ownerId;
        public InvokeRemoveDoorCommand(int ownerId)
        {
            _ownerId = ownerId;
        }

        public async Task Execute(WorldChannel ctx)
        {
            if (ctx.PlayerDoors.Remove(_ownerId, out var door) && door != null)
            {
                await door.Destroy();
            }
        }
    }
}
