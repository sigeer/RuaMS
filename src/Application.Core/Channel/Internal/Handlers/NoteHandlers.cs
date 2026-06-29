using Application.Core.Models;
using Application.Shared.Message;
using AutoMapper;
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

            protected override Task HandleMessage(SendNoteResponse res)
            {
                return _server.SendToPlayerAsync(res.ReceiverChannel, res.ReceiverId, async chr =>
                {
                    await chr.SendPacket(new ShowNotesPacket(chr.Client, _server.Mapper.Map<List<NoteObject>>(res.List)));
                });
            }

            protected override SendNoteResponse Parse(ByteString data) => SendNoteResponse.Parser.ParseFrom(data);
        }
    }
}
