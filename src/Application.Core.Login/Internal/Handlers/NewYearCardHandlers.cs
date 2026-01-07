using Application.Core.Login.Services;
using Application.Shared.Internal;
using Application.Shared.Message;
using Dto;
using Google.Protobuf;
using InvitationProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Login.Internal.Handlers
{
    internal class NewYearCardHandlers
    {
        internal class ReceiveHandler : InternalSessionMasterHandler<ReceiveNewYearCardRequest>
        {
            public ReceiveHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.ReceiveNewYearCard;

            protected override async Task HandleAsync(ReceiveNewYearCardRequest message, CancellationToken cancellationToken = default)
            {
                await _server.NewYearCardManager.ReceiveNewYearCard(message);
            }

            protected override ReceiveNewYearCardRequest Parse(ByteString content) => ReceiveNewYearCardRequest.Parser.ParseFrom(content);
        }

        internal class DiscardHandler : InternalSessionMasterHandler<DiscardNewYearCardRequest>
        {
            public DiscardHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.DiscardNewYearCard;

            protected override async Task HandleAsync(DiscardNewYearCardRequest message, CancellationToken cancellationToken = default)
            {
                await _server.NewYearCardManager.DiscardNewYearCard(message);
            }

            protected override DiscardNewYearCardRequest Parse(ByteString content) => DiscardNewYearCardRequest.Parser.ParseFrom(content);
        }
        internal class SendHandler : InternalSessionMasterHandler<SendNewYearCardRequest>
        {
            public SendHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.SendNewYearCard;

            protected override async Task HandleAsync(SendNewYearCardRequest message, CancellationToken cancellationToken = default)
            {
                await _server.NewYearCardManager.SendNewYearCard(message);
            }

            protected override SendNewYearCardRequest Parse(ByteString content) => SendNewYearCardRequest.Parser.ParseFrom(content);
        }
    }
}
