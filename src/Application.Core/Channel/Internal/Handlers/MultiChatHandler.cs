using Application.Core.Channel.Commands;
using Application.Shared.Message;
using Google.Protobuf;
using tools;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class MultiChatHandler : InternalSessionChannelHandler<ProtoModel.MultiChatMessage>
    {
        public MultiChatHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.MultiChat;

        protected override Task HandleMessage(ProtoModel.MultiChatMessage data)
        {
            return _server.PushChannelCommandAsync(new InvokeChannelBroadcastCommand(data.Receivers, PacketCreator.multiChat(data.FromName, data.Text, data.Type)));
        }

        protected override ProtoModel.MultiChatMessage Parse(ByteString content) => ProtoModel.MultiChatMessage.Parser.ParseFrom(content);
    }
}
