using Application.Core.Login.Commands;
using Application.Shared.Internal;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Login.Internal
{
    public class MessageDispatcherNew
    {
        MasterServer _server;
        Dictionary<int, IInternalSessionMasterHandler> _handlers = new();
        public MessageDispatcherNew(MasterServer server)
        {
            _server = server;

            _handlers = _server.ServiceProvider.GetServices<IInternalSessionMasterHandler>().ToDictionary(x => x.MessageId);
        }


        public void DispatchAsync(int msgId, ByteString content)
        {
            if (_handlers.TryGetValue(msgId, out var handler))
            {
                _server.Post(new HandleChannelPacketCommand(handler, content));
            }
            else
            {
                throw new InvalidOperationException($"未注册的消息: {msgId}");
            }
        }
    }
}
