using Application.Shared.Invitations;
using Dto;
using Microsoft.Extensions.Logging;
using net.server.guild;
using static Application.Core.Channel.Internal.Handlers.NoteHandlers;

namespace Application.Core.Channel.Invitation
{
    internal class AllianceInviteChannelHandler : InviteChannelHandler
    {
        public AllianceInviteChannelHandler(WorldChannel server, ILogger<InviteChannelHandler> logger) : base(server, InviteTypes.Alliance, logger)
        {
        }

        public override void OnInvitationAnswered(InvitationProto.AnswerInviteResponse data)
        {
            var result = (InviteResultType)data.Result;
            if (result != InviteResultType.ACCEPTED)
            {
                var senderActor = _server.getPlayerStorage().GetCharacterActor(data.SenderPlayerId);
                if (senderActor != null)
                {
                    senderActor.Send(m =>
                    {
                        string msg = "";
                        if (result == InviteResultType.DENIED)
                            msg = "[" + data.TargetName + "] guild has denied your guild alliance invitation.";
                        if (result == InviteResultType.NOT_FOUND)
                            msg = "The guild alliance request has not been accepted, since the invitation expired.";
                        m.getCharacterById(data.SenderPlayerId)?.dropMessage(5, msg);
                    });
                }
            }
        }

        public override void OnInvitationCreated(InvitationProto.CreateInviteResponse data)
        {
            var code = (InviteResponseCode)data.Code;
            if (code == InviteResponseCode.Success)
            {
                var receiverActor = _server.getPlayerStorage().GetCharacterActor(data.ReceivePlayerId);
                if (receiverActor != null)
                {
                    receiverActor.Send(m =>
                    {
                        var receiver = m.getCharacterById(data.ReceivePlayerId);
                        receiver?.sendPacket(GuildPackets.allianceInvite(data.Key, receiver));
                    });
                }

            }
            else
            {
                var senderActor = _server.getPlayerStorage().GetCharacterActor(data.SenderPlayerId);
                if (senderActor != null)
                {
                    senderActor.Send(m =>
                    {
                        var sender = m.getCharacterById(data.SenderPlayerId);
                        if (sender != null)
                        {
                            switch (code)
                            {
                                case InviteResponseCode.Alliance_AlreadyInAlliance:
                                    sender.dropMessage(5, "The entered guild is already registered on a guild alliance.");
                                    break;
                                case InviteResponseCode.Alliance_GuildNotFound:
                                    sender.dropMessage(5, "The entered guild does not exist.");
                                    break;
                                case InviteResponseCode.Alliance_GuildLeaderNotFound:
                                    sender.dropMessage(5, "The master of the guild that you offered an invitation is currently not online.");
                                    break;
                                case InviteResponseCode.Alliance_CapacityFull:
                                    sender.dropMessage(5, "Your alliance cannot comport any more guilds at the moment.");
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
}
