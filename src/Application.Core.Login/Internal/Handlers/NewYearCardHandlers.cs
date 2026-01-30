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

            protected override void HandleMessage(ReceiveNewYearCardRequest message)
            {
                _ = _server.NewYearCardManager.ReceiveNewYearCard(message);
            }

            protected override ReceiveNewYearCardRequest Parse(ByteString content) => ReceiveNewYearCardRequest.Parser.ParseFrom(content);
        }

        internal class DiscardHandler : InternalSessionMasterHandler<DiscardNewYearCardRequest>
        {
            public DiscardHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.DiscardNewYearCard;

            protected override void HandleMessage(DiscardNewYearCardRequest message)
            {
                _ = _server.NewYearCardManager.DiscardNewYearCard(message);
            }

            protected override DiscardNewYearCardRequest Parse(ByteString content) => DiscardNewYearCardRequest.Parser.ParseFrom(content);
        }
        internal class SendHandler : InternalSessionMasterHandler<SendNewYearCardRequest>
        {
            public SendHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.SendNewYearCard;

            protected override void HandleMessage(SendNewYearCardRequest message)
            {
                _ = _server.NewYearCardManager.SendNewYearCard(message);
            }

            protected override SendNewYearCardRequest Parse(ByteString content) => SendNewYearCardRequest.Parser.ParseFrom(content);
        }
    }
}
