using Application.Shared.Invitations;
using Dto;
using Microsoft.Extensions.Logging;
using net.server.guild;

namespace Application.Core.Channel.Invitation
{
    internal class AllianceInviteChannelHandler : InviteChannelHandler
    {
        public AllianceInviteChannelHandler(WorldChannelServer server, ILogger<InviteChannelHandler> logger) : base(server, InviteTypes.Alliance, logger)
        {
        }

        public override void OnInvitationAnswered(InvitationProto.AnswerInviteResponse data)
        {
            var result = (InviteResultType)data.Result;
            if (result != InviteResultType.ACCEPTED)
            {
                var sender = _server.FindPlayerById(data.SenderPlayerId);
                if (sender != null)
                {
                    string msg = "";
                    if (result == InviteResultType.DENIED)
                        msg = "[" + data.TargetName + "] guild has denied your guild alliance invitation.";
                    if (result == InviteResultType.NOT_FOUND)
                        msg = "The guild alliance request has not been accepted, since the invitation expired.";
                    sender.dropMessage(5, msg);
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
                    receiver.sendPacket(GuildPackets.allianceInvite(data.Key, receiver));
                }

            }
            else
            {
                var sender = _server.FindPlayerById(data.SenderPlayerId);
                if (sender != null)
                {
                    // sender.dropMessage(5, "The master of the guild that you offered an invitation is currently managing another invite.");
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
            }
        }
    }
}
