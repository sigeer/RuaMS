using Application.Shared.Invitations;
using Dto;
using Microsoft.Extensions.Logging;
using net.server.guild;

namespace Application.Core.Channel.Invitation
{
    internal class GuildInviteChannelHandler : InviteChannelHandler
    {
        public GuildInviteChannelHandler(WorldChannelServer server, ILogger<InviteChannelHandler> logger) : base(server, InviteTypes.Guild, logger)
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
                    var code = result == InviteResultType.DENIED ? GuildResponse.DENIED_INVITE : GuildResponse.NOT_FOUND_INVITE;
                    sender.sendPacket(code.getPacket(data.ReceivePlayerName));
                }
            }
        }

        public override void OnInvitationCreated(CreateInviteResponse data)
        {
            var code = (GuildResponse)data.Code;
            if (code == GuildResponse.Success)
            {
                var receiver = _server.FindPlayerById(data.ReceivePlayerId);
                if (receiver != null)
                {
                    receiver.sendPacket(GuildPackets.guildInvite(data.Key, data.SenderPlayerName));
                }

            }
            else
            {
                var sender = _server.FindPlayerById(data.SenderPlayerId);
                if (sender != null)
                {
                    sender.sendPacket(code.getPacket(data.ReceivePlayerName));
                }
            }
        }
    }
}
