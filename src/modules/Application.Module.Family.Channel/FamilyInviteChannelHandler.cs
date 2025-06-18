using Application.Core.Channel;
using Application.Core.Channel.Invitation;
using Application.Module.Family.Channel.Net.Packets;
using Application.Module.Family.Common;
using Application.Shared.Invitations;
using Dto;
using Microsoft.Extensions.Logging;

namespace Application.Module.Family.Channel
{
    internal class FamilyInviteChannelHandler : InviteChannelHandler
    {
        public FamilyInviteChannelHandler(WorldChannelServer server, ILogger<InviteChannelHandler> logger) : base(server, Constants.InviteType_Family, logger)
        {
        }

        public override void OnInvitationAnswered(AnswerInviteResponse data)
        {
            var result = (InviteResultType)data.Result;
            if (result != InviteResultType.ACCEPTED)
            {
                var sender = _server.FindPlayerById(data.SenderPlayerId);
                if (sender != null)
                {
                    sender.sendPacket(FamilyPacketCreator.sendFamilyJoinResponse(false, data.ReceivePlayerName));
                }
            }
        }

        public override void OnInvitationCreated(CreateInviteResponse data)
        {
            var code = (InviteResponseCode)data.Code;
            if (code == InviteResponseCode.Success)
            {
                var receiver = _server.FindPlayerById(data.ReceivePlayerId);
                if (receiver != null)
                {
                    receiver.sendPacket(FamilyPacketCreator.sendFamilyInvite(data.SenderPlayerId, data.SenderPlayerName));
                }

                var sender = _server.FindPlayerById(data.SenderPlayerId);
                if (sender != null)
                {
                    sender.dropMessage("The invite has been sent.");
                }
            }
            else
            {
                var sender = _server.FindPlayerById(data.SenderPlayerId);
                if (sender != null)
                {
                    switch (code)
                    {
                        case InviteResponseCode.MANAGING_INVITE:
                            sender.sendPacket(FamilyPacketCreator.sendFamilyMessage(73, 0));
                            break;
                        default:
                            _logger.LogCritical("预料之外的邀请回调: Type:{Type}, Code: {Code}", Type, code);
                            break;
                    }
                }
            }
        }
    }
}
