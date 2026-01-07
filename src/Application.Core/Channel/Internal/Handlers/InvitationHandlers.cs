using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf;
using InvitationProto;
using System;
using System.Collections.Generic;
using System.Text;
using XmlWzReader;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class InvitationHandlers
    {
        public class SendInvite : InternalSessionChannelHandler<CreateInviteResponse>
        {
            public SendInvite(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnInvitationSent;

            protected override Task HandleAsync(CreateInviteResponse res, CancellationToken cancellationToken = default)
            {
                _server.InviteChannelHandlerRegistry.GetHandler(res.Type)?.OnInvitationCreated(res);
                return Task.CompletedTask;
            }

            protected override CreateInviteResponse Parse(ByteString data) => CreateInviteResponse.Parser.ParseFrom(data);
        }

        public class AnswerInvite : InternalSessionChannelHandler<AnswerInviteResponse>
        {
            public AnswerInvite(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnInvitationAnswered;

            protected override Task HandleAsync(AnswerInviteResponse res, CancellationToken cancellationToken = default)
            {
                _server.InviteChannelHandlerRegistry.GetHandler(res.Type)?.OnInvitationAnswered(res);
                return Task.CompletedTask;
            }

            protected override AnswerInviteResponse Parse(ByteString data) => AnswerInviteResponse.Parser.ParseFrom(data);
        }
    }
}
