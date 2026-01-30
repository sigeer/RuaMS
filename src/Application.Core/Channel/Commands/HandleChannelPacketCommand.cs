namespace Application.Core.Channel.Commands
{
    internal class HandleChannelPacketCommand : IWorldChannelCommand
    {
        ISocketClient _client;
        InPacket _inPacket;

        public HandleChannelPacketCommand(ISocketClient client, InPacket inPacket)
        {
            _client = client;
            _inPacket = inPacket;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _client.ProcessPacket(_inPacket);
        }
    }
}
