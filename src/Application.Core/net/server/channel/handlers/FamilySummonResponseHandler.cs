

using Application.Core.Game.Maps;
using client;
using net.packet;
using net.server.coordinator.world;
using tools;

namespace net.server.channel.handlers;

public class FamilySummonResponseHandler : AbstractPacketHandler
{

    public override void HandlePacket(InPacket p, IClient c)
    {
        if (!YamlConfig.config.server.USE_FAMILY_SYSTEM)
        {
            return;
        }
        p.readString(); //family name
        bool accept = p.readByte() != 0;
        InviteResult inviteResult = InviteCoordinator.answerInvite(InviteType.FAMILY_SUMMON, c.OnlinedCharacter.getId(), c.OnlinedCharacter, accept);
        if (inviteResult.result == InviteResultType.NOT_FOUND)
        {
            return;
        }
        var inviter = inviteResult.from;
        var inviterEntry = inviter.getFamilyEntry();
        if (inviterEntry == null)
        {
            return;
        }
        var map = (IMap)inviteResult.paramsValue[0];
        if (accept && inviter.getMap() == map)
        { //cancel if inviter has changed maps
            c.OnlinedCharacter.changeMap(map, map.getPortal(0));
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
