using Application.Core.Login.Models;
using Google.Protobuf;
using System.Threading.Tasks;

namespace Application.Core.Login.ServerTransports
{
    public abstract class MasterServerTransportBase
    {
        protected readonly MasterServer _server;
        public MasterServerTransportBase(MasterServer masterServer)
        {
            this._server = masterServer;
        }

        /// <summary>
        /// 只需要给部分玩家发送消息，仅需要找到这部分玩家的频道服务器
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <param name="playerIdArray"></param>
        public void SendMessage<TMessage>(string messageType, TMessage message, IEnumerable<int> playerIdArray) where TMessage : IMessage
        {
            if (playerIdArray.Count() == 0)
                return;

            var serverGroups = _server.GroupPlayer(playerIdArray);
            foreach (var group in serverGroups)
            {
                group.BroadcastMessage(messageType, message);
            }
        }

        public void SendMessage<TMessage>(string messageType, TMessage message, IEnumerable<CharacterLiveObject> playerIdArray) where TMessage : IMessage
        {
            if (playerIdArray.Count() == 0)
                return;

            var serverGroups = _server.GroupPlayer(playerIdArray);
            foreach (var group in serverGroups)
            {
                group.BroadcastMessage(messageType, message);
            }
        }


        public void BroadcastMessage<TMessage>(string messageType, TMessage message) where TMessage : IMessage
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(messageType, message);
            }
        }
        public async Task BroadcastMessageN<TMessage>(int messageType, TMessage message) where TMessage : IMessage
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                await server.BroadcastMessageN(messageType, message);
            }
        }

    }
}
