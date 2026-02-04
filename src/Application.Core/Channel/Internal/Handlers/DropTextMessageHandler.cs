using Application.Core.Channel.Commands;
using Application.Shared.Message;
using Google.Protobuf;
using MessageProto;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class DropTextMessageHandler : InternalSessionChannelHandler<DropMessageBroadcast>
    {
        public DropTextMessageHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.DropTextMessage;

        protected override void HandleMessage(DropMessageBroadcast msg)
        {
            _server.PushChannelCommand(new InvokeMultiDropMessageCommand(msg.Receivers, msg.Type, msg.Message));
        }

        protected override DropMessageBroadcast Parse(ByteString content) => DropMessageBroadcast.Parser.ParseFrom(content);
    }
}
