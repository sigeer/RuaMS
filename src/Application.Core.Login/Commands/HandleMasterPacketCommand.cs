namespace Application.Core.Login.Commands
{
    internal class HandleMasterPacketCommand : IMasterCommand
    {
        ISocketClient _client;
        InPacket _inPacket;

        public HandleMasterPacketCommand(ISocketClient client, InPacket inPacket)
        {
            _client = client;
            _inPacket = inPacket;
        }

        public void Execute(MasterCommandContext ctx)
        {
            _client.ProcessPacket(_inPacket);
        }
    }
}
