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

        public override int MessageId => ChannelRecvCode.MultiChat;

        protected override Task HandleAsync(MultiChatMessage data, CancellationToken cancellationToken = default)
        {
            foreach (var cid in data.Receivers)
            {
                var chr = _server.FindPlayerById(cid);
                if (chr != null && !chr.isAwayFromWorld())
                {
                    chr.sendPacket(PacketCreator.multiChat(data.FromName, data.Text, data.Type));
                }
            }
            return Task.CompletedTask;
        }

        protected override MultiChatMessage Parse(ByteString content) => MultiChatMessage.Parser.ParseFrom(content);
    }
}
