using Application.Core.Channel.Net;
using Application.Core.Client;
using Application.Module.Family.Channel.Net.Packets;
using Application.Shared.Net;
using tools;

namespace Application.Module.Family.Channel.Net.Handlers;

public class FamilyPreceptsHandler : ChannelHandlerBase
{
    readonly FamilyManager _familyManager;

    public FamilyPreceptsHandler(FamilyManager familyManager)
    {
        _familyManager = familyManager;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var family = _familyManager.GetFamilyByPlayerId(c.OnlinedCharacter.Id);
        if (family == null)
        {
            return;
        }
        if (family.getLeader().Id != c.OnlinedCharacter.Id)
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
        c.sendPacket(FamilyPacketCreator.getFamilyInfo(family.getEntryByID(c.OnlinedCharacter.Id)));
    }

}
