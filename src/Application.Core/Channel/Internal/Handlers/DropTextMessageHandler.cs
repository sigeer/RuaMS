using Application.Shared.Message;
using Google.Protobuf;
using MessageProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class DropTextMessageHandler : InternalSessionHandler<DropMessageBroadcast>
    {
        public DropTextMessageHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => ChannelRecvCode.DropTextMessage;

        protected override Task HandleAsync(DropMessageBroadcast msg, CancellationToken cancellationToken = default)
        {
            if (msg.Receivers.Contains(-1))
            {
                foreach (var player in _server.PlayerStorage.getAllCharacters())
                {
                    player.dropMessage(msg.Type, msg.Message);
                }
            }
            else
            {
                foreach (var id in msg.Receivers)
                {
                    _server.PlayerStorage.getCharacterById(id)?.dropMessage(msg.Type, msg.Message);
                }
            }

            return Task.CompletedTask;
        }

        protected override DropMessageBroadcast Parse(ByteString content) => DropMessageBroadcast.Parser.ParseFrom(content);
    }
}
