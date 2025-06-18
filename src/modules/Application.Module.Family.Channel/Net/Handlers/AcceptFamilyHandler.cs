using Application.Core.Channel.Net;
using Application.Core.Client;
using Application.Core.Game.Invites;
using Application.Module.Family.Channel.Net.Packets;
using Application.Shared.Net;
using Microsoft.Extensions.Logging;

namespace Application.Module.Family.Channel.Net.Handlers;

public class AcceptFamilyHandler : ChannelHandlerBase
{
    protected ILogger<AcceptFamilyHandler> _logger;
    readonly FamilyManager _familyManager;

    public AcceptFamilyHandler(ILogger<AcceptFamilyHandler> logger, FamilyManager familyManager)
    {
        _logger = logger;
        _familyManager = familyManager;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        int inviterId = p.readInt();
        p.readString();
        bool accept = p.readByte() != 0;
        // string inviterName = slea.readMapleAsciiString();
        var inviter = c.getWorldServer().getPlayerStorage().getCharacterById(inviterId);
        if (inviter != null && inviter.IsOnlined)
        {
            InviteResult inviteResult = InviteType.FAMILY.AnswerInvite(c.OnlinedCharacter.getId(), inviter.Id, accept);
            if (inviteResult.Result == InviteResultType.NOT_FOUND)
            {
                return; //was never invited. (or expired on server only somehow?)
            }
            if (accept)
            {
                _familyManager.AcceptInvite(inviterId, chr.Id);
            }
            else
            {
                inviter.sendPacket(FamilyPacketCreator.sendFamilyJoinResponse(false, chr.getName()));
            }
        }
        c.sendPacket(FamilyPacketCreator.sendFamilyMessage(0, 0));
    }
}
