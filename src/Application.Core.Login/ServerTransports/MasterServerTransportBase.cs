using Application.Core.Login.Models;
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

        /// <summary>
        /// 只需要给部分玩家发送消息，仅需要找到这部分玩家的频道服务器
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <param name="playerIdArray"></param>
        public void SendMessage<TMessage>(string messageType, TMessage message, params int[] playerIdArray) where TMessage : IMessage
        {
            if (playerIdArray.Length == 0)
                return;

            var serverGroups = _server.GroupPlayer(playerIdArray);
            foreach (var group in serverGroups)
            {
                group.Key.BroadcastMessage(messageType, message);
            }
        }

        /// <summary>
        /// 只需要给部分玩家发送消息，仅需要找到这部分玩家的频道服务器
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <param name="playerIdArray"></param>
        public void SendMessage<TMessage>(string messageType, TMessage message, params PlayerChannelPair[] playerIdArray) where TMessage : IMessage
        {
            var serverGroups = _server.GroupPlayer(playerIdArray);
            foreach (var group in serverGroups)
            {
                group.Key.BroadcastMessage(messageType, message);
            }
        }

        public void BroadcastMessage<TMessage>(string messageType, TMessage message) where TMessage : IMessage
        {
            foreach (var server in _server.ChannelServerList.Values)
            {
                server.BroadcastMessage(messageType, message);
            }
        }

    }
}
