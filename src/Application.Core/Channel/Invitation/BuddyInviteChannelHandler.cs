using Application.Shared.Invitations;
using Dto;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Invitation
{
    internal class BuddyInviteChannelHandler : InviteChannelHandler
    {
        public BuddyInviteChannelHandler(WorldChannelServer server, ILogger<InviteChannelHandler> logger) : base(server, InviteTypes.Buddy, logger)
        {
        }

        public override void OnInvitationAnswered(AnswerInviteResponse data)
        {
        }

        public override void OnInvitationCreated(CreateInviteResponse data)
        {
            var code = (InviteResponseCode)data.Code;
            if (code == InviteResponseCode.Success)
            {
                var receiver = _server.FindPlayerById(data.ReceivePlayerId);
                if (receiver != null && !receiver.BuddyList.Contains(data.SenderPlayerId))
                {
                    receiver.sendPacket(PacketCreator.requestBuddylistAdd(data.SenderPlayerId, data.ReceivePlayerId, data.SenderPlayerName));
                }
            }
        }
    }
}
