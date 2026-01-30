using Application.Core.Channel.Commands;
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

            public override int MessageId => (int)ChannelRecvCode.OnNewYearCardReceived;

            protected override void HandleMessage(ReceiveNewYearCardResponse res)
            {
                _server.PushChannelCommand(new InvokeNewYearcardReceivedCommand(res));
            }

            protected override ReceiveNewYearCardResponse Parse(ByteString data) => ReceiveNewYearCardResponse.Parser.ParseFrom(data);
        }

        public class SendCard : InternalSessionChannelHandler<SendNewYearCardResponse>
        {
            public SendCard(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnNewYearCardSent;

            protected override void HandleMessage(SendNewYearCardResponse res)
            {
                _server.PushChannelCommand(new InvokeNewYearCardSendCommand(res));
            }

            protected override SendNewYearCardResponse Parse(ByteString data) => SendNewYearCardResponse.Parser.ParseFrom(data);
        }

        public class Notify : InternalSessionChannelHandler<NewYearCardNotifyDto>
        {
            public Notify(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnNewYearCardNotify;

            protected override void HandleMessage(NewYearCardNotifyDto res)
            {
                _server.PushChannelCommand(new InvokeNewYearCardNotifyCommand(res));
            }

            protected override NewYearCardNotifyDto Parse(ByteString data) => NewYearCardNotifyDto.Parser.ParseFrom(data);
        }

        public class Discard : InternalSessionHandler<WorldChannelServer, DiscardNewYearCardResponse>
        {
            public Discard(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnNewYearCardDiscard;

            protected override void HandleMessage(DiscardNewYearCardResponse res)
            {
                _server.PushChannelCommand(new InvokeNewYearCardDiscardCommand(res));
            }

            protected override DiscardNewYearCardResponse Parse(ByteString data) => DiscardNewYearCardResponse.Parser.ParseFrom(data);
        }
    }
}
