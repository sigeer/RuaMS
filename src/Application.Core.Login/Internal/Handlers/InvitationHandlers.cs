using Application.Core.Login.Services;
using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class InvitationHandlers
    {
        internal class CreateInvitationHandler : InternalSessionMasterHandler<ProtoService.CreateInviteRequest>
        {
            readonly InvitationService _invitationService;
            public CreateInvitationHandler(MasterServer server, InvitationService invitationService) : base(server)
            {
                _invitationService = invitationService;
            }

            public override int MessageId => (int)ChannelSendCode.SendInvitation;

            protected override Task HandleMessage(ProtoService.CreateInviteRequest message)
            {
                return _invitationService.AddInvitation(message);
            }

            protected override ProtoService.CreateInviteRequest Parse(ByteString content) => ProtoService.CreateInviteRequest.Parser.ParseFrom(content);
        }

        internal class AnswerInvitation : InternalSessionMasterHandler<ProtoService.AnswerInviteRequest>
        {
            readonly InvitationService _invitationService;
            public AnswerInvitation(MasterServer server, InvitationService invitationService) : base(server)
            {
                _invitationService = invitationService;
            }

            public override int MessageId => (int)ChannelSendCode.AnswerInvitation;

            protected override Task HandleMessage(ProtoService.AnswerInviteRequest message)
            {
                return _invitationService.AnswerInvitation(message);
            }

            protected override ProtoService.AnswerInviteRequest Parse(ByteString content) => ProtoService.AnswerInviteRequest.Parser.ParseFrom(content);
        }
    }
}
