using Google.Protobuf;

namespace Application.Core.Login.ServerTransports
{
    public abstract class MasterServerTransportBase
    {
        protected readonly MasterServer _server;
        public MasterServerTransportBase(MasterServer masterServer)
        {
            this._server = masterServer;
        }

        public async Task SendMessageN<TMessage>(int messageType, TMessage message, IEnumerable<int> playerIdArray) where TMessage : IMessage
        {
            if (playerIdArray.Count() == 0)
                return;

            var serverGroups = _server.GroupPlayer(playerIdArray);
            foreach (var group in serverGroups)
            {
                await group.SendMessage(messageType, message);
            }
        }


        public async Task BroadcastMessageN<TMessage>(int messageType, TMessage message) where TMessage : IMessage
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                await server.SendMessage(messageType, message);
            }
        }
        public async Task BroadcastMessageN(int messageType)
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                await server.SendMessage(messageType);
            }
        }
    }
}
