using Application.Shared.Internal;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Channel.Internal
{
    public abstract class InternalSessionChannelHandler<TMessage> : InternalSessionHandler<WorldChannelServer, TMessage>, IInternalSessionChannelHandler where TMessage : IMessage
    {
        protected InternalSessionChannelHandler(WorldChannelServer server) : base(server)
        {
        }
    }

    public abstract class InternalSessionChannelEmptyHandler : InternalSessionChannelHandler<Empty>
    {
        static Empty Empty = new Empty();
        protected InternalSessionChannelEmptyHandler(WorldChannelServer server) : base(server)
        {
        }

        protected override Empty Parse(ByteString data) => Empty;
    }

    public interface IInternalSessionChannelHandler : IInternalSessionHandler
    {

    }

}
