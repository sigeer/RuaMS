

using Application.Core.Game.Invites;
using Application.Core.Game.Maps;
using client;
using net.packet;
using net.server.coordinator.world;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class FamilySummonResponseHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        if (!YamlConfig.config.server.USE_FAMILY_SYSTEM)
        {
            return;
        }
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
        var inviterEntry = inviter.getFamilyEntry();
        if (inviterEntry == null)
        {
            return;
        }
        if (accept && inviter.getMap() == request.Map)
        { //cancel if inviter has changed maps
            c.OnlinedCharacter.changeMap(request.Map, request.Map.getPortal(0));
        }
        else
        {
            inviterEntry.refundEntitlement(FamilyEntitlement.SUMMON_FAMILY);
            inviterEntry.gainReputation(FamilyEntitlement.SUMMON_FAMILY.getRepCost(), false); //refund rep cost if declined
            inviter.sendPacket(PacketCreator.getFamilyInfo(inviterEntry));
            inviter.dropMessage(5, c.OnlinedCharacter.getName() + " has denied the summon request.");
        }
    }

}
