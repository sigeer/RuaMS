using Application.Core.Models;
using Application.Shared.Message;
using Dto;
using Google.Protobuf;
using net.packet.outs;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class NoteHandlers
    {
        public class Receive : InternalSessionChannelHandler<SendNoteResponse>
        {
            public Receive(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.InvokeNoteMessage;

            protected override void HandleMessage(SendNoteResponse res)
            {
                var channel = _server.GetChannelActor(res.ReceiverChannel);
                if (channel != null)
                {
                    channel.Send(x =>
                    {
                        var actor = x.getPlayerStorage().GetCharacterActor(res.ReceiverId);
                        if (actor != null)
                        {
                            actor.Send(async map =>
                            {
                                var chr = map.getCharacterById(res.ReceiverId);
                                if (chr != null)
                                {
                                    await chr.SendPacket(new ShowNotesPacket(chr.Client, x.Mapper.Map<List<NoteObject>>(res.List)));
                                }
                            });
                        }
                    });
                }
            }

            protected override SendNoteResponse Parse(ByteString data) => SendNoteResponse.Parser.ParseFrom(data);
        }
    }
}
