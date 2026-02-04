
using Application.Core.Channel.Commands;
using Application.Core.Channel.Internal;
using Application.Shared.Internal;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Channel.Message
{
    public class MessageDispatcherNew
    {
        WorldChannelServer _server;
        Dictionary<int, IInternalSessionChannelHandler> _handlers = new();
        public MessageDispatcherNew(WorldChannelServer server)
        {
            _server = server;

            _handlers = _server.ServiceProvider.GetServices<IInternalSessionChannelHandler>().ToDictionary(x => x.MessageId);
        }


        public void DispatchAsync(int msgId, ByteString content)
        {
            if (_handlers.TryGetValue(msgId, out var handler))
            {
                _server.Post(new HandleMasterPacketCommand(handler, content));
            }
            else
            {
                throw new InvalidOperationException($"未注册的消息: {msgId}");
            }
        }
    }

}
