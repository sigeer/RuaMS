

using client;
using net.packet;
using net.server.coordinator.world;
using server.maps;
using tools;

namespace net.server.channel.handlers;

public class FamilySummonResponseHandler : AbstractPacketHandler
{

    public override void handlePacket(InPacket p, Client c)
    {
        if (!YamlConfig.config.server.USE_FAMILY_SYSTEM)
        {
            return;
        }
        p.readString(); //family name
        bool accept = p.readByte() != 0;
        InviteResult inviteResult = InviteCoordinator.answerInvite(InviteType.FAMILY_SUMMON, c.getPlayer().getId(), c.getPlayer(), accept);
        if (inviteResult.result == InviteResultType.NOT_FOUND)
        {
            return;
        }
        Character inviter = inviteResult.from;
        FamilyEntry inviterEntry = inviter.getFamilyEntry();
        if (inviterEntry == null)
        {
            return;
        }
        MapleMap map = (MapleMap)inviteResult.paramsValue[0];
        if (accept && inviter.getMap() == map)
        { //cancel if inviter has changed maps
            c.getPlayer().changeMap(map, map.getPortal(0));
        }
        else
        {
            inviterEntry.refundEntitlement(FamilyEntitlement.SUMMON_FAMILY);
            inviterEntry.gainReputation(FamilyEntitlement.SUMMON_FAMILY.getRepCost(), false); //refund rep cost if declined
            inviter.sendPacket(PacketCreator.getFamilyInfo(inviterEntry));
            inviter.dropMessage(5, c.getPlayer().getName() + " has denied the summon request.");
        }
    }

}
