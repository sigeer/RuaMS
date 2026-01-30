using Application.Core.Channel.Commands;
using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf;
using MessageProto;
using tools;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class MultiChatHandler : InternalSessionChannelHandler<MultiChatMessage>
    {
        public MultiChatHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.MultiChat;

        protected override void HandleMessage(MultiChatMessage data)
        {
            _server.PushChannelCommand(new InvokeChannelBroadcastCommand(data.Receivers, PacketCreator.multiChat(data.FromName, data.Text, data.Type)));
        }

        protected override MultiChatMessage Parse(ByteString content) => MultiChatMessage.Parser.ParseFrom(content);
    }
}
