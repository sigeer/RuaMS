using Application.Core.Channel.Net.Packets;
using Application.Resources.Messages;
using Application.Shared.Invitations;
using Dto;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Invitation
{
    internal class PartyInviteChannelHandler : InviteChannelHandler
    {
        public PartyInviteChannelHandler(WorldChannelServer server, ILogger<InviteChannelHandler> logger) : base(server, InviteTypes.Party, logger)
        {
        }

        public override void OnInvitationAnswered(InvitationProto.AnswerInviteResponse data)
        {
            var result = (InviteResultType)data.Result;

            if (result == InviteResultType.DENIED)
            {
                var sender = _server.FindPlayerById(data.SenderPlayerId);
                if (sender != null)
                {
                    sender.sendPacket(TeamPacketCreator.partyStatusMessage(23, data.ReceivePlayerName));
                }
            }

            if (result == InviteResultType.NOT_FOUND)
            {
                var receiver = _server.FindPlayerById(data.ReceivePlayerId);
                if (receiver != null)
                {
                    receiver.Pink(nameof(ClientMessage.Team_InvitationExpired));
                }
            }
        }

        public override void OnInvitationCreated(InvitationProto.CreateInviteResponse data)
        {
            var code = (InviteResponseCode)data.Code;
            if (code == InviteResponseCode.Success)
            {
                var receiver = _server.FindPlayerById(data.ReceivePlayerId);
                if (receiver != null)
                {
                    receiver.sendPacket(TeamPacketCreator.partyInvite(data.Key, data.SenderPlayerName));
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
                            sender.sendPacket(TeamPacketCreator.partyStatusMessage(22, data.ReceivePlayerName));
                            break;
                        case InviteResponseCode.InviteesNotFound:
                            sender.sendPacket(TeamPacketCreator.partyStatusMessage(19));
                            break;
                        case InviteResponseCode.Team_AlreadyInTeam:
                            sender.sendPacket(TeamPacketCreator.AlreadInTeam());
                            break;
                        case InviteResponseCode.Team_CapacityFull:
                            sender.sendPacket(TeamPacketCreator.TeamFullCapacity());
                            break;
                        case InviteResponseCode.Team_BeginnerLimit:
                            sender.Pink(nameof(ClientMessage.Team_Invitation_NoviceLimit));
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
