using Application.Core.Channel.Internal;
using Google.Protobuf;

namespace Application.Core.Channel.Commands
{
    internal class HandleMasterPacketCommand : IChannelCommand
    {
        IInternalSessionChannelHandler _handler;
        ByteString _content;

        public HandleMasterPacketCommand(IInternalSessionChannelHandler handler, ByteString content)
        {
            _handler = handler;
            _content = content;
        }

        public void Execute(ChannelNodeCommandContext ctx)
        {
            _handler.Handle(_content);
        }
    }
}
