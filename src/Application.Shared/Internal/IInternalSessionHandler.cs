using Application.Shared.Servers;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Application.Shared.Internal
{
    public interface IInternalSessionHandler
    {
        int MessageId { get; }
        Task Handle(ByteString message, CancellationToken cancellationToken = default);
    }

    public abstract class InternalSessionHandler<TServer, TMessage> : IInternalSessionHandler where TMessage : IMessage where TServer : IServerBase<IServerTransport>
    {
        protected TServer _server;
        protected InternalSessionHandler(TServer server)
        {
            _server = server;
        }
        public abstract int MessageId { get; }

        protected abstract Task HandleAsync(TMessage res, CancellationToken cancellationToken = default);

        protected abstract TMessage Parse(ByteString data);

        public Task Handle(ByteString message, CancellationToken cancellationToken = default)
        {
            return HandleAsync(Parse(message));
        }
    }

    public abstract class InternalSessionEmptyHandler<TServer> : InternalSessionHandler<TServer, Empty> where TServer : IServerBase<IServerTransport>
    {
        static Empty Empty = new Empty();
        protected InternalSessionEmptyHandler(TServer server) : base(server)
        {
        }

        protected override Empty Parse(ByteString content) => Empty;
    }
}
