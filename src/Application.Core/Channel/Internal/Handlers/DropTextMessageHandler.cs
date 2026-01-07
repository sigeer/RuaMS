using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf;
using MessageProto;
using System;
using System.Collections.Generic;
using System.Text;
using tools;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class DropTextMessageHandler : InternalSessionChannelHandler<DropMessageBroadcast>
    {
        public DropTextMessageHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => (int)ChannelRecvCode.DropTextMessage;

        protected override Task HandleAsync(DropMessageBroadcast msg, CancellationToken cancellationToken = default)
        {
            if (msg.Receivers.Contains(-1))
            {
                foreach (var player in _server.PlayerStorage.getAllCharacters())
                {
                    if (msg.Type > 0)
                    {
                        player.dropMessage(msg.Type, msg.Message);
                    }
                    else if(msg.Type == -1)
                    {
                        player.Yellow(msg.Message);
                    }
                    else if (msg.Type == -2)
                    {
                        player.sendPacket(PacketCreator.earnTitleMessage(msg.Message));
                    }
                }
            }
            else
            {
                foreach (var id in msg.Receivers)
                {
                    var player = _server.PlayerStorage.getCharacterById(id);
                    if (player != null)
                    {
                        if (msg.Type > 0)
                        {
                            player.dropMessage(msg.Type, msg.Message);
                        }
                        else if (msg.Type == -1)
                        {
                            player.Yellow(msg.Message);
                        }
                        else if (msg.Type == -2)
                        {
                            player.sendPacket(PacketCreator.earnTitleMessage(msg.Message));
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        protected override DropMessageBroadcast Parse(ByteString content) => DropMessageBroadcast.Parser.ParseFrom(content);
    }
}
