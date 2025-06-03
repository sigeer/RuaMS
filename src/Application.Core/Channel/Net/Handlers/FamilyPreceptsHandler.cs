using tools;

namespace Application.Core.Channel.Net.Handlers;

public class FamilyPreceptsHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var family = c.OnlinedCharacter.getFamily();
        if (family == null)
        {
            return;
        }
        if (family.getLeader().getChr() != c.OnlinedCharacter)
        {
            return; //only the leader can set the precepts
        }
        string newPrecepts = p.readString();
        if (newPrecepts.Length > 200)
        {
            return;
        }
        family.setMessage(newPrecepts, true);
        //family.broadcastFamilyInfoUpdate(); //probably don't need to broadcast for this?
        c.sendPacket(PacketCreator.getFamilyInfo(c.OnlinedCharacter.getFamilyEntry()));
    }

}
