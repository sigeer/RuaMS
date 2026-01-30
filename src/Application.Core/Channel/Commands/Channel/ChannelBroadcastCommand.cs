namespace Application.Core.Channel.Commands
{

    internal class InvokeChannelBroadcastCommand : IWorldChannelCommand
    {
        IEnumerable<int> _receivers;
        Packet _packet;

        public InvokeChannelBroadcastCommand(IEnumerable<int> receivers, Packet packet)
        {
            _receivers = receivers;
            _packet = packet;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (_receivers.Contains(-1))
            {
                foreach (var player in ctx.WorldChannel.getPlayerStorage().getAllCharacters())
                {
                    player.sendPacket(_packet);
                }
            }
            else
            {
                foreach (var item in _receivers)
                {
                    var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(item);
                    if (chr != null)
                    {
                        chr.sendPacket(_packet);
                    }
                }
            }

            return;
        }
    }
}
