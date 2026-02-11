using Application.Core.Channel.Internal;
using Application.Utility.Performance;
using Google.Protobuf;
using System.Diagnostics;

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
            using var activity = GameMetrics.ActivitySource.StartActivity(nameof(HandleMasterPacketCommand));
            activity?.SetTag("Handler", _handler.GetType().Name);

            _handler.Handle(_content);
        }
    }
}
