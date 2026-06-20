using Application.Core.Channel.Net.Packets;
using Application.Resources.Messages;
using Application.Shared.Invitations;
using Microsoft.Extensions.Logging;

namespace Application.Core.Channel.Invitation
{
    internal class PartyInviteChannelHandler : InviteChannelHandler
    {
        public PartyInviteChannelHandler(WorldChannel server, ILogger<InviteChannelHandler> logger) : base(server, InviteTypes.Party, logger)
        {
        }

        public override void OnInvitationAnswered(InvitationProto.AnswerInviteResponse data)
        {
            var result = (InviteResultType)data.Result;

            if (result == InviteResultType.DENIED)
            {
                var senderActor = _server.getPlayerStorage().GetCharacterActor(data.SenderPlayerId);
                senderActor?.Send(async m =>
                {
                    var chr = m.getCharacterById(data.SenderPlayerId);
                    if (chr != null)
                    {
                        await chr.SendPacket(TeamPacketCreator.partyStatusMessage(23, data.ReceivePlayerName));
                    }
                });
            }

            if (result == InviteResultType.NOT_FOUND)
            {
                var receiverActor = _server.getPlayerStorage().GetCharacterActor(data.ReceivePlayerId);
                receiverActor?.Send(async m =>
                {
                    var chr = m.getCharacterById(data.ReceivePlayerId);
                    if (chr != null)
                    {
                        await chr.Pink(nameof(ClientMessage.Team_InvitationExpired));
                    }
                });
            }
        }

        public override void OnInvitationCreated(InvitationProto.CreateInviteResponse data)
        {
            var code = (InviteResponseCode)data.Code;
            if (code == InviteResponseCode.Success)
            {
                var receiverActor = _server.getPlayerStorage().GetCharacterActor(data.ReceivePlayerId);
                receiverActor?.Send(async m =>
                {
                    var chr = m.getCharacterById(data.ReceivePlayerId);
                    if (chr != null)
                    {
                        await chr.SendPacket(TeamPacketCreator.partyInvite(data.Key, data.SenderPlayerName));
                    }
                });
            }
            else
            {
                var senderActor = _server.getPlayerStorage().GetCharacterActor(data.SenderPlayerId);
                senderActor?.Send(async m =>
                {
                    var sender = m.getCharacterById(data.SenderPlayerId);
                    if (sender != null)
                    {
                        switch (code)
                        {
                            case InviteResponseCode.MANAGING_INVITE:
                                await sender.SendPacket(TeamPacketCreator.partyStatusMessage(22, data.ReceivePlayerName));
                                break;
                            case InviteResponseCode.InviteesNotFound:
                                await sender.SendPacket(TeamPacketCreator.partyStatusMessage(19));
                                break;
                            case InviteResponseCode.Team_AlreadyInTeam:
                                await sender.SendPacket(TeamPacketCreator.AlreadInTeam());
                                break;
                            case InviteResponseCode.Team_CapacityFull:
                                await sender.SendPacket(TeamPacketCreator.TeamFullCapacity());
                                break;
                            case InviteResponseCode.Team_BeginnerLimit:
                                await sender.Pink(nameof(ClientMessage.Team_Invitation_NoviceLimit));
                                break;
                            default:
                                _logger.LogCritical("预料之外的邀请回调: Type:{Type}, Code: {Code}", Type, code);
                                break;
                        }
                    }
                });

            }
        }
    }
}
