using Application.Shared.Internal;
using Application.Shared.Message;
using Dto;
using Google.Protobuf;
using InvitationProto;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class NewYearCardHandlers
    {
        public class ReceiveCard : InternalSessionChannelHandler<ReceiveNewYearCardResponse>
        {
            public ReceiveCard(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.OnNewYearCardReceived;

            protected override Task HandleAsync(ReceiveNewYearCardResponse res, CancellationToken cancellationToken = default)
            {
                _server.NewYearCardService.OnNewYearCardReceived(res);
                return Task.CompletedTask;
            }

            protected override ReceiveNewYearCardResponse Parse(ByteString data) => ReceiveNewYearCardResponse.Parser.ParseFrom(data);
        }

        public class SendCard : InternalSessionChannelHandler<SendNewYearCardResponse>
        {
            public SendCard(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.OnNewYearCardSent;

            protected override Task HandleAsync(SendNewYearCardResponse res, CancellationToken cancellationToken = default)
            {
                _server.NewYearCardService.OnNewYearCardSend(res);
                return Task.CompletedTask;
            }

            protected override SendNewYearCardResponse Parse(ByteString data) => SendNewYearCardResponse.Parser.ParseFrom(data);
        }

        public class Notify : InternalSessionChannelHandler<NewYearCardNotifyDto>
        {
            public Notify(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.OnNewYearCardNotify;

            protected override Task HandleAsync(NewYearCardNotifyDto res, CancellationToken cancellationToken = default)
            {
                _server.NewYearCardService.OnNewYearCardNotify(res);
                return Task.CompletedTask;
            }

            protected override NewYearCardNotifyDto Parse(ByteString data) => NewYearCardNotifyDto.Parser.ParseFrom(data);
        }

        public class Discard : InternalSessionHandler<WorldChannelServer, DiscardNewYearCardResponse>
        {
            public Discard(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.OnNewYearCardDiscard;

            protected override Task HandleAsync(DiscardNewYearCardResponse res, CancellationToken cancellationToken = default)
            {
                _server.NewYearCardService.OnNewYearCardDiscard(res);
                return Task.CompletedTask;
            }

            protected override DiscardNewYearCardResponse Parse(ByteString data) => DiscardNewYearCardResponse.Parser.ParseFrom(data);
        }
    }
}
