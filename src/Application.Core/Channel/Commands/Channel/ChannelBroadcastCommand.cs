namespace Application.Core.Channel.Commands
{

    internal class InvokeChannelBroadcastCommand : IWorldChannelAsyncCommand
    {
        public string Name => nameof(InvokeChannelBroadcastCommand);
        IEnumerable<int> _receivers;
        Packet _packet;

        public InvokeChannelBroadcastCommand(IEnumerable<int> receivers, Packet packet)
        {
            _receivers = receivers;
            _packet = packet;
        }

        public async Task Execute(WorldChannel ctx)
        {
            if (_receivers.Contains(-1))
            {
                foreach (var player in ctx.getPlayerStorage().getAllCharacters())
                {
                    await player.SendPacket(_packet);
                }
            }
            else
            {
                foreach (var item in _receivers)
                {
                    var chr = ctx.getPlayerStorage().getCharacterById(item);
                    if (chr != null)
                    {
                        await chr.SendPacket(_packet);
                    }
                }
            }

            return;
        }
    }
}
