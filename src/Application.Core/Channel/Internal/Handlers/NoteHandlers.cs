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

            public override int MessageId => ChannelRecvCode.InvokeNoteMessage;

            protected override Task HandleAsync(SendNoteResponse res, CancellationToken cancellationToken = default)
            {
                _server.NoteService.OnNoteReceived(res);
                return Task.CompletedTask;
            }

            protected override SendNoteResponse Parse(ByteString data) => SendNoteResponse.Parser.ParseFrom(data);
        }
    }
}
