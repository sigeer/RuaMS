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
