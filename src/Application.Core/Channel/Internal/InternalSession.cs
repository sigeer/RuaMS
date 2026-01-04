using Application.Shared.Message;
using BaseProto;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Channel.Internal
{
    public class InternalSession : IDisposable
    {
        WorldChannelServer _server;
        AsyncDuplexStreamingCall<BaseProto.PacketWrapper, BaseProto.PacketWrapper>? _streamingCall;

        bool _connected = false;
        public InternalSession(WorldChannelServer server)
        {
            _server = server;
        }

        public void Connect(AsyncDuplexStreamingCall<BaseProto.PacketWrapper, BaseProto.PacketWrapper> call)
        {
            if (_connected)
                return;

            _connected = true;
            _streamingCall = call;
            _ = Task.Run(async () =>
            {
                await foreach (var msg in _streamingCall.ResponseStream.ReadAllAsync())
                {
                    await _server.MessageDispatcherV.DispatchAsync(msg.EventId, msg.Data);
                }
            });
        }

        public async Task DisconnectAsync()
        {
            if (_streamingCall == null)
                return;

            await _streamingCall.RequestStream.CompleteAsync();
            Dispose();
        }
        public async Task SendAsync(int type, IMessage message, CancellationToken cancellationToken = default)
        {
            if (_streamingCall == null)
                throw new BusinessServerOfflineException();

            await _streamingCall.RequestStream.WriteAsync(new PacketWrapper
            {
                EventId = type,
                Data = message.ToByteString()
            }, cancellationToken);
        }
        public async Task SendAsync(int type, CancellationToken cancellationToken = default)
        {
            if (_streamingCall == null)
                throw new BusinessServerOfflineException();

            await _streamingCall.RequestStream.WriteAsync(new PacketWrapper
            {
                EventId = type,
            }, cancellationToken);
        }
        public void Send(int type)
        {
            SendAsync(type).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _streamingCall?.Dispose();
            _streamingCall = null;
            _connected = false;
        }
    }
}
