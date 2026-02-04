using Application.Core.Channel.Commands;
using Application.Shared.Internal;
using Application.Shared.Message;
using Dto;
using Google.Protobuf;
using LifeProto;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;

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
                var channel = _server.GetChannel(res.ReceiverChannel);
                if (channel != null)
                {
                    channel.Post(new InvokeNoteReceiveCommand(res));
                }
            }

            protected override SendNoteResponse Parse(ByteString data) => SendNoteResponse.Parser.ParseFrom(data);
        }
    }
}
