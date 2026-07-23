using Application.Core.Login.Services;
using Application.Shared.Message;
using Google.Protobuf;
using InvitationProto;

namespace Application.Core.Login.Internal.Handlers
{
    internal class InvitationHandlers
    {
        internal class CreateInvitationHandler : InternalSessionMasterHandler<CreateInviteRequest>
        {
            readonly InvitationService _invitationService;
            public CreateInvitationHandler(MasterServer server, InvitationService invitationService) : base(server)
            {
                _invitationService = invitationService;
            }

            public override int MessageId => (int)ChannelSendCode.SendInvitation;

            protected override Task HandleMessage(CreateInviteRequest message)
            {
                return _invitationService.AddInvitation(message);
            }

            protected override CreateInviteRequest Parse(ByteString content) => CreateInviteRequest.Parser.ParseFrom(content);
        }

        internal class AnswerInvitation : InternalSessionMasterHandler<AnswerInviteRequest>
        {
            readonly InvitationService _invitationService;
            public AnswerInvitation(MasterServer server, InvitationService invitationService) : base(server)
            {
                _invitationService = invitationService;
            }

            public override int MessageId => (int)ChannelSendCode.AnswerInvitation;

            protected override Task HandleMessage(AnswerInviteRequest message)
            {
                return _invitationService.AnswerInvitation(message);
            }

            protected override AnswerInviteRequest Parse(ByteString content) => AnswerInviteRequest.Parser.ParseFrom(content);
        }
    }
}
