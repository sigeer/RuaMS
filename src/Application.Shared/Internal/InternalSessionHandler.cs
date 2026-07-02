using Application.Shared.Servers;
using Google.Protobuf;
using System.Threading.Tasks;

namespace Application.Shared.Internal
{
    public abstract class InternalSessionHandler<TServer, TMessage> : IInternalSessionHandler where TMessage : IMessage where TServer : IServerBase<IServerTransport>
    {
        protected TServer _server;
        protected InternalSessionHandler(TServer server)
        {
            _server = server;
        }
        public abstract int MessageId { get; }

        protected abstract Task HandleMessage(TMessage res);

        protected abstract TMessage Parse(ByteString data);

        public async Task Handle(ByteString message)
        {
            await HandleMessage(Parse(message));
        }
    }
}
