using Application.Shared.Invitations;
using Dto;
using Microsoft.Extensions.Logging;
using net.server.guild;

namespace Application.Core.Channel.Invitation
{
    internal class GuildInviteChannelHandler : InviteChannelHandler
    {
        public GuildInviteChannelHandler(WorldChannel server, ILogger<InviteChannelHandler> logger) : base(server, InviteTypes.Guild, logger)
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
                    var code = result == InviteResultType.DENIED ? GuildResponse.DENIED_INVITE : GuildResponse.NOT_FOUND_INVITE;
                    senderActor.Send(m =>
                    {
                        m.getCharacterById(data.SenderPlayerId)?.sendPacket(code.getPacket(data.ReceivePlayerName));
                    });
                }
            }
        }

        public override void OnInvitationCreated(InvitationProto.CreateInviteResponse data)
        {
            var code = (GuildResponse)data.Code;
            if (code == GuildResponse.Success)
            {
                var receiverActor = _server.getPlayerStorage().GetCharacterActor(data.ReceivePlayerId);
                receiverActor?.Send(m =>
                {
                    m.getCharacterById(data.ReceivePlayerId)?.sendPacket(GuildPackets.guildInvite(data.Key, data.SenderPlayerName));
                });

            }
            else
            {
                var senderActor = _server.getPlayerStorage().GetCharacterActor(data.SenderPlayerId);
                senderActor?.Send(m =>
                {
                    m.getCharacterById(data.SenderPlayerId)?.sendPacket(code.getPacket(data.ReceivePlayerName));
                });
            }
        }
    }
}
