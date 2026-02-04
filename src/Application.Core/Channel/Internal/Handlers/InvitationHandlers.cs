using Application.Core.Channel.Commands;
using Application.Shared.Message;
using Google.Protobuf;
using InvitationProto;

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

            protected override void HandleMessage(CreateInviteResponse res)
            {
                _server.PushChannelCommand(new InvokeCreateInviteCommand(res));
            }

            protected override CreateInviteResponse Parse(ByteString data) => CreateInviteResponse.Parser.ParseFrom(data);
        }

        public class AnswerInvite : InternalSessionChannelHandler<AnswerInviteResponse>
        {
            public AnswerInvite(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnInvitationAnswered;

            protected override void HandleMessage(AnswerInviteResponse res)
            {
                _server.PushChannelCommand(new InvokeAnswerInviteCommand(res));
            }

            protected override AnswerInviteResponse Parse(ByteString data) => AnswerInviteResponse.Parser.ParseFrom(data);
        }
    }
}
