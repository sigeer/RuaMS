using Application.Core.Channel.Commands;
using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class DropTextMessageHandler : InternalSessionChannelHandler<ProtoModel.DropMessageBroadcastProto>
    {
        public DropTextMessageHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.DropTextMessage;

        protected override Task HandleMessage(ProtoModel.DropMessageBroadcastProto msg)
        {
            _server.PushChannelCommand(new InvokeMultiDropMessageCommand(msg.Receivers, msg.Type, msg.Message));
            return Task.CompletedTask;
        }

        protected override ProtoModel.DropMessageBroadcastProto Parse(ByteString content) => ProtoModel.DropMessageBroadcastProto.Parser.ParseFrom(content);
    }
}
