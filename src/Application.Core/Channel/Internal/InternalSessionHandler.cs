using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Channel.Internal
{
    /// <summary>
    /// 高频或服务器等级的跨频道处理
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class InternalSessionHandler<TMessage> : IInternalSessionHandler where TMessage : IMessage
    {
        protected WorldChannelServer _server;
        protected InternalSessionHandler(WorldChannelServer server)
        {
            _server = server;
        }
        public abstract int MessageId { get; }

        protected abstract Task HandleAsync(TMessage message, CancellationToken cancellationToken = default);

        protected abstract TMessage Parse(ByteString content);

        public Task Handle(ByteString message, CancellationToken cancellationToken = default)
        {
            return HandleAsync(Parse(message));
        }
    }

    public abstract class InternalSessionEmptyHandler : InternalSessionHandler<Empty>
    {
        static Empty Empty = new Empty();
        protected InternalSessionEmptyHandler(WorldChannelServer server) : base(server)
        {
        }

        protected override Empty Parse(ByteString content) => Empty;
    }

}
