
using Application.Core.Channel.Internal;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Channel.Message
{
    public class MessageDispatcherNew
    {
        WorldChannelServer _server;
        Dictionary<int, IInternalSessionHandler> _handlers = new();
        public MessageDispatcherNew(WorldChannelServer server)
        {
            _server = server;

            _handlers = _server.ServiceProvider.GetServices<IInternalSessionHandler>().ToDictionary(x => x.MessageId);
        }


        public async Task DispatchAsync(int msgId, ByteString content, CancellationToken cancellationToken = default)
        {
            if (_handlers.TryGetValue(msgId, out var handler))
            {
                await handler.Handle(content, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException($"未注册的消息: {msgId}");
            }
        }
    }

}
