using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class InvitationHandlers
    {
        public class SendInvite : InternalSessionChannelHandler<ProtoService.CreateInviteResponse>
        {
            public SendInvite(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnInvitationSent;

            protected override Task HandleMessage(ProtoService.CreateInviteResponse res)
            {
                return _server.BroadcastAsync(w =>
                {
                    w.InviteChannelHandlerRegistry.GetHandler(res.Type)?.OnInvitationCreated(res);
                });
            }

            protected override ProtoService.CreateInviteResponse Parse(ByteString data) => ProtoService.CreateInviteResponse.Parser.ParseFrom(data);
        }

        public class AnswerInvite : InternalSessionChannelHandler<ProtoService.AnswerInviteResponse>
        {
            public AnswerInvite(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnInvitationAnswered;

            protected override Task HandleMessage(ProtoService.AnswerInviteResponse res)
            {
                return _server.BroadcastAsync(w =>
                {
                    w.InviteChannelHandlerRegistry.GetHandler(res.Type)?.OnInvitationAnswered(res); ;
                });
            }

            protected override ProtoService.AnswerInviteResponse Parse(ByteString data) => ProtoService.AnswerInviteResponse.Parser.ParseFrom(data);
        }
    }
}
