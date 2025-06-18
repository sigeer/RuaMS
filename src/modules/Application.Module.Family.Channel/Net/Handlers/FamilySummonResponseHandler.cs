using Application.Core.Channel.Net;
using Application.Core.Client;
using Application.Core.Game.Invites;
using Application.Module.Family.Channel.Net.Packets;
using Application.Module.Family.Common;
using Application.Shared.Net;

namespace Application.Module.Family.Channel.Net.Handlers;

public class FamilySummonResponseHandler : ChannelHandlerBase
{
    readonly FamilyManager _familyManager;

    public FamilySummonResponseHandler(FamilyManager familyManager)
    {
        _familyManager = familyManager;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        p.readString(); //family name
        bool accept = p.readByte() != 0;
        InviteResult inviteResult = InviteType.FAMILY_SUMMON.AnswerInvite(c.OnlinedCharacter.getId(), c.OnlinedCharacter.getId(), accept);
        if (inviteResult.Result == InviteResultType.NOT_FOUND)
        {
            return;
        }

        var request = inviteResult.Request as FamilySummonInviteRequest;
        if (request == null)
            return;

        var inviter = request.From;
        var inviterEntry = _familyManager.GetFamilyByPlayerId(inviter.Id)?.getEntryByID(inviter.Id);
        if (inviterEntry == null)
        {
            return;
        }
        if (accept && inviter.getMap() == request.Map)
        {
            //cancel if inviter has changed maps
            c.OnlinedCharacter.changeMap(request.Map, request.Map.getPortal(0));
        }
        else
        {
            _familyManager.DeclineSummon(c.OnlinedCharacter);

        }
    }

}
