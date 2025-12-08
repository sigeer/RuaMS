using Application.Core.Channel.Internal.Handlers;
using Application.Core.Game.Players;
using Application.Shared.Message;
using BaseProto;
using Config;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Core.Channel.Internal
{
    public class InternalSession: IDisposable
    {
        WorldChannelServer _server;
        AsyncDuplexStreamingCall<BaseProto.PacketWrapper, BaseProto.PacketWrapper>? _streamingCall;
        Dictionary<int, IInternalSessionHandler> _handlers = new();

        bool _connected = false;
        public InternalSession(WorldChannelServer server)
        {
            _server = server;

            _handlers = _server.ServiceProvider.GetServices<IInternalSessionHandler>().ToDictionary(x => x.MessageId);
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
                    await Handle(msg.EventId, msg.Data);
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

        public async Task Handle(int msgId, ByteString content, CancellationToken cancellationToken = default)
        {
            if (_handlers.TryGetValue(msgId, out var handler))
            {
                await handler.Handle(content, cancellationToken);
            }
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
        public void Send(int type, IMessage message)
        {
            SendAsync(type, message).GetAwaiter().GetResult();
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

        public void SendMultiChat(int type, string fromName, string text, int[] receivers)
        {
            var data = new MessageProto.MultiChatMessage { Type = type, FromName = fromName, Text = text };
            data.Receivers.AddRange(receivers);
            Send(ChannelSendCode.MultiChat, data);
        }
    }
}
