using Application.Core.Login.Services;
using Application.Shared.Internal;
using Application.Shared.Message;
using Google.Protobuf;
using InvitationProto;
using MessageProto;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;

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

            protected override void HandleMessage(CreateInviteRequest message)
            {
                _ = _invitationService.AddInvitation(message);
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

            protected override void HandleMessage(AnswerInviteRequest message)
            {
                _ = _invitationService.AnswerInvitation(message);
            }

            protected override AnswerInviteRequest Parse(ByteString content) => AnswerInviteRequest.Parser.ParseFrom(content);
        }
    }
}
