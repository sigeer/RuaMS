using Application.Core.Channel.Internal;
using Application.Utility.Performance;
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


        public Task DispatchAsync(int msgId, ByteString content)
        {
            if (_handlers.TryGetValue(msgId, out var handler))
            {
                return _server.Send(s =>
                {
                    using var activity = GameMetrics.ActivitySource.StartActivity("HandleMasterPacket");
                    activity?.SetTag("Handler", handler.GetType().Name);

                    _ = handler.Handle(content);
                });
            }
            else
            {
                throw new InvalidOperationException($"未注册的消息: {msgId}");
            }
        }
    }

}
