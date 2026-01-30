using Application.Shared.Servers;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Application.Shared.Internal
{
    public interface IInternalSessionHandler
    {
        int MessageId { get; }
        void Handle(ByteString message);
    }

    public abstract class InternalSessionHandler<TServer, TMessage> : IInternalSessionHandler where TMessage : IMessage where TServer : IServerBase<IServerTransport>
    {
        protected TServer _server;
        protected InternalSessionHandler(TServer server)
        {
            _server = server;
        }
        public abstract int MessageId { get; }

        protected abstract void HandleMessage(TMessage res);

        protected abstract TMessage Parse(ByteString data);

        public void Handle(ByteString message)
        {
            HandleMessage(Parse(message));
        }
    }
}
